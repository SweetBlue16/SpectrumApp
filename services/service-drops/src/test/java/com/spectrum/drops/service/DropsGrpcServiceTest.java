package com.spectrum.drops.service;

import com.spectrum.drops.grpc.*;
import com.spectrum.drops.model.AccessKey;
import com.spectrum.drops.model.Event;
import com.spectrum.drops.repository.AccessKeyRepository;
import com.spectrum.drops.repository.EventRepository;
import io.grpc.stub.StreamObserver;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.mongodb.core.FindAndModifyOptions;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.query.Query;
import org.springframework.data.mongodb.core.query.UpdateDefinition;

import java.time.Instant;
import java.util.List;
import java.util.Optional;

import static org.mockito.ArgumentMatchers.eq;
import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
public class DropsGrpcServiceTest {

    @Mock
    private EventRepository eventRepository;

    @Mock
    private AccessKeyRepository accessKeyRepository;

    @Mock
    private MongoTemplate mongoTemplate;

    @Mock
    private StreamObserver<ClaimKeyResponse> claimResponseObserver;

    @InjectMocks
    private DropsGrpcService dropsGrpcService;

    @BeforeEach
    void setUp() {
        reset(eventRepository, accessKeyRepository, mongoTemplate, claimResponseObserver);
    }

    @Test
    void claimAccessKeyWhenValidRequestAndKeysAvailableShouldReturnKey() {
        ClaimKeyRequest request = ClaimKeyRequest.newBuilder()
                .setUserId("user-123")
                .setEventId("event-1")
                .build();

        Event activeEvent = new Event();
        activeEvent.setId("event-1");
        activeEvent.setStatus("ACTIVE");
        activeEvent.setEndDate(Instant.now().toEpochMilli() + 100000);

        when(eventRepository.findById("event-1")).thenReturn(Optional.of(activeEvent));
        when(accessKeyRepository.existsByEventIdAndClaimedByUserId("event-1", "user-123")).thenReturn(false);

        AccessKey wonKey = new AccessKey();
        wonKey.setKeyCode("STEAM-WIN-123");
        wonKey.setClaimedAt(Instant.now().toEpochMilli());

        when(mongoTemplate.findAndModify(
                any(Query.class),
                any(UpdateDefinition.class),
                any(FindAndModifyOptions.class),
                eq(AccessKey.class)))
                .thenReturn(wonKey);

        ArgumentCaptor<ClaimKeyResponse> responseCaptor = ArgumentCaptor.forClass(ClaimKeyResponse.class);

        dropsGrpcService.claimAccessKey(request, claimResponseObserver);

        verify(claimResponseObserver).onNext(responseCaptor.capture());
        assertTrue(responseCaptor.getValue().getSuccess());
        assertEquals("STEAM-WIN-123", responseCaptor.getValue().getAccessKeyCode());
        verify(claimResponseObserver).onCompleted();
    }

    @Test
    void claimAccessKeyWhenUserAlreadyClaimedShouldReturnError() {
        ClaimKeyRequest request = ClaimKeyRequest.newBuilder()
                .setUserId("user-123")
                .setEventId("event-1")
                .build();

        when(accessKeyRepository.existsByEventIdAndClaimedByUserId("event-1", "user-123")).thenReturn(true);

        ArgumentCaptor<ClaimKeyResponse> responseCaptor = ArgumentCaptor.forClass(ClaimKeyResponse.class);

        dropsGrpcService.claimAccessKey(request, claimResponseObserver);

        verify(mongoTemplate, never()).findAndModify(
                any(Query.class),
                any(UpdateDefinition.class),
                any(FindAndModifyOptions.class),
                any());
        verify(claimResponseObserver).onNext(responseCaptor.capture());
        assertFalse(responseCaptor.getValue().getSuccess());
    }

    @Test
    void claimAccessKeyWhenKeysDepletedShouldReturnError() {
        ClaimKeyRequest request = ClaimKeyRequest.newBuilder()
                .setUserId("user-123")
                .setEventId("event-1")
                .build();

        Event activeEvent = new Event();
        activeEvent.setId("event-1");
        activeEvent.setStatus("ACTIVE");
        activeEvent.setEndDate(Instant.now().toEpochMilli() + 100000);

        when(eventRepository.findById("event-1")).thenReturn(Optional.of(activeEvent));
        when(accessKeyRepository.existsByEventIdAndClaimedByUserId(anyString(), anyString())).thenReturn(false);

        when(mongoTemplate.findAndModify(any(Query.class), any(UpdateDefinition.class), any(FindAndModifyOptions.class), eq(AccessKey.class)))
                .thenReturn(null);

        ArgumentCaptor<ClaimKeyResponse> responseCaptor = ArgumentCaptor.forClass(ClaimKeyResponse.class);

        dropsGrpcService.claimAccessKey(request, claimResponseObserver);

        verify(claimResponseObserver).onNext(responseCaptor.capture());
        assertFalse(responseCaptor.getValue().getSuccess());
        assertEquals("", responseCaptor.getValue().getAccessKeyCode());
    }

    @Test
    void getEventStatusWhenEventExistsShouldReturnDetails() {
        GetEventRequest request = GetEventRequest.newBuilder()
                .setEventId("event-123")
                .build();

        Event mockEvent = new Event();
        mockEvent.setId("event-123");
        mockEvent.setKeysAvailable(50);
        mockEvent.setKeysTotal(100);
        mockEvent.setStatus("ACTIVE");
        mockEvent.setEndDate(1700000000L);

        when(eventRepository.findById("event-123")).thenReturn(Optional.of(mockEvent));

        StreamObserver<EventStatusResponse> statusObserver = mock(StreamObserver.class);
        ArgumentCaptor<EventStatusResponse> responseCaptor = ArgumentCaptor.forClass(EventStatusResponse.class);

        dropsGrpcService.getEventStatus(request, statusObserver);

        verify(statusObserver).onNext(responseCaptor.capture());
        EventStatusResponse response = responseCaptor.getValue();
        assertEquals("ACTIVE", response.getStatus());
        assertEquals(50, response.getKeysAvailable());
        assertEquals(100, response.getKeysTotal());
        verify(statusObserver).onCompleted();
    }

    @Test
    void getEventStatusWhenEventNotFoundShouldReturnNotFoundStatus() {
        GetEventRequest request = GetEventRequest.newBuilder().setEventId("unknown-event").build();
        when(eventRepository.findById("unknown-event")).thenReturn(Optional.empty());

        StreamObserver<EventStatusResponse> statusObserver = mock(StreamObserver.class);
        ArgumentCaptor<EventStatusResponse> responseCaptor = ArgumentCaptor.forClass(EventStatusResponse.class);

        dropsGrpcService.getEventStatus(request, statusObserver);

        verify(statusObserver).onNext(responseCaptor.capture());
        assertEquals("NOT_FOUND", responseCaptor.getValue().getStatus());
        verify(statusObserver).onCompleted();
    }

    @Test
    void getWonKeysWhenKeysExistShouldReturnKeysWithTitles() {
        WonKeysRequest request = WonKeysRequest.newBuilder().setUserId("user-1").build();

        AccessKey key = new AccessKey();
        key.setEventId("event-1");
        key.setKeyCode("STEAM-123");
        key.setClaimedAt(1600000000L);

        Event event = new Event();
        event.setId("event-1");
        event.setGameTitle("Halo 3");

        when(accessKeyRepository.findByClaimedByUserId("user-1")).thenReturn(List.of(key));
        when(eventRepository.findAllById(List.of("event-1"))).thenReturn(List.of(event));

        StreamObserver<WonKeysResponse> wonKeysObserver = mock(StreamObserver.class);
        ArgumentCaptor<WonKeysResponse> responseCaptor = ArgumentCaptor.forClass(WonKeysResponse.class);

        dropsGrpcService.getWonKeys(request, wonKeysObserver);

        verify(wonKeysObserver).onNext(responseCaptor.capture());
        WonKeysResponse response = responseCaptor.getValue();
        assertEquals(1, response.getWonKeysCount());
        assertEquals("Halo 3", response.getWonKeys(0).getGameTitle());
        assertEquals("STEAM-123", response.getWonKeys(0).getAccessKeyCode());
        verify(wonKeysObserver).onCompleted();
    }

    @Test
    void getWonKeysWhenUserIdIsBlankShouldReturnDefaultInstance() {
        WonKeysRequest request = WonKeysRequest.newBuilder().setUserId("").build();
        StreamObserver<WonKeysResponse> wonKeysObserver = mock(StreamObserver.class);
        ArgumentCaptor<WonKeysResponse> responseCaptor = ArgumentCaptor.forClass(WonKeysResponse.class);

        dropsGrpcService.getWonKeys(request, wonKeysObserver);

        verify(accessKeyRepository, never()).findByClaimedByUserId(anyString());
        verify(wonKeysObserver).onNext(responseCaptor.capture());
        assertEquals(0, responseCaptor.getValue().getWonKeysCount());
        verify(wonKeysObserver).onCompleted();
    }

    @Test
    void createEventWhenValidRequestShouldSaveEventAndKeysAndReturnsSuccess() {
        CreateEventRequest request = CreateEventRequest.newBuilder()
                .setGameTitle("Gears of War")
                .setCoverImageUrl("url")
                .setEndDate(1800000000L)
                .addAccessKeys("KEY1")
                .addAccessKeys("KEY2")
                .build();

        Event savedEvent = new Event();
        savedEvent.setId("new-event-id");
        savedEvent.setKeysTotal(2);

        when(eventRepository.save(any(Event.class))).thenReturn(savedEvent);

        StreamObserver<EventActionResponse> createObserver = mock(StreamObserver.class);
        ArgumentCaptor<EventActionResponse> responseCaptor = ArgumentCaptor.forClass(EventActionResponse.class);

        dropsGrpcService.createEvent(request, createObserver);

        verify(eventRepository).save(any(Event.class));
        verify(accessKeyRepository).saveAll(anyList());

        verify(createObserver).onNext(responseCaptor.capture());
        assertTrue(responseCaptor.getValue().getSuccess());
        assertEquals("new-event-id", responseCaptor.getValue().getEventId());
        verify(createObserver).onCompleted();
    }

    @Test
    void createEventWhenNoKeysProvidedShouldReturnError() {
        CreateEventRequest request = CreateEventRequest.newBuilder()
                .setGameTitle("Empty Event")
                .build();

        StreamObserver<EventActionResponse> createObserver = mock(StreamObserver.class);
        ArgumentCaptor<EventActionResponse> responseCaptor = ArgumentCaptor.forClass(EventActionResponse.class);

        dropsGrpcService.createEvent(request, createObserver);

        verify(eventRepository, never()).save(any());
        verify(createObserver).onNext(responseCaptor.capture());
        assertFalse(responseCaptor.getValue().getSuccess());
        assertTrue(responseCaptor.getValue().getMessage().contains("required"));
        verify(createObserver).onCompleted();
    }

    @Test
    void updateEventWhenEventExistsShouldUpdateAndReturnSuccess() {
        UpdateEventRequest request = UpdateEventRequest.newBuilder()
                .setEventId("event-1")
                .setGameTitle("Updated Title")
                .setCoverImageUrl("new-url")
                .setEndDate(1900000000L)
                .setStatus("INACTIVE")
                .build();

        Event existingEvent = new Event();
        existingEvent.setId("event-1");

        when(eventRepository.findById("event-1")).thenReturn(Optional.of(existingEvent));

        StreamObserver<EventActionResponse> updateObserver = mock(StreamObserver.class);
        ArgumentCaptor<EventActionResponse> responseCaptor = ArgumentCaptor.forClass(EventActionResponse.class);

        dropsGrpcService.updateEvent(request, updateObserver);

        ArgumentCaptor<Event> eventCaptor = ArgumentCaptor.forClass(Event.class);
        verify(eventRepository).save(eventCaptor.capture());

        Event savedEvent = eventCaptor.getValue();
        assertEquals("Updated Title", savedEvent.getGameTitle());
        assertEquals("INACTIVE", savedEvent.getStatus());

        verify(updateObserver).onNext(responseCaptor.capture());
        assertTrue(responseCaptor.getValue().getSuccess());
        verify(updateObserver).onCompleted();
    }

    @Test
    void updateEventWhenEventNotFoundShouldReturnError() {
        UpdateEventRequest request = UpdateEventRequest.newBuilder()
                .setEventId("unknown-event")
                .build();

        when(eventRepository.findById("unknown-event")).thenReturn(Optional.empty());

        StreamObserver<EventActionResponse> updateObserver = mock(StreamObserver.class);
        ArgumentCaptor<EventActionResponse> responseCaptor = ArgumentCaptor.forClass(EventActionResponse.class);

        dropsGrpcService.updateEvent(request, updateObserver);

        verify(eventRepository, never()).save(any());
        verify(updateObserver).onNext(responseCaptor.capture());
        assertFalse(responseCaptor.getValue().getSuccess());
        assertEquals("Event not found.", responseCaptor.getValue().getMessage());
        verify(updateObserver).onCompleted();
    }
}
