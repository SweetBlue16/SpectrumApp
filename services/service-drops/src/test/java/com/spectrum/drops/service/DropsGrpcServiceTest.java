package com.spectrum.drops.service;

import com.spectrum.drops.grpc.*;
import com.spectrum.drops.model.Event;
import com.spectrum.drops.model.EventParticipant;
import com.spectrum.drops.repository.EventParticipantRepository;
import com.spectrum.drops.repository.EventRepository;
import io.grpc.stub.StreamObserver;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.mongodb.core.FindAndModifyOptions;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.query.Query;
import org.springframework.data.mongodb.core.query.UpdateDefinition;

import java.time.Instant;
import java.util.Optional;
import java.util.Queue;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicBoolean;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.*;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class DropsGrpcServiceTest {

    @Mock
    private EventRepository eventRepository;

    @Mock
    private EventParticipantRepository participantRepository;

    @Mock
    private MongoTemplate mongoTemplate;

    @InjectMocks
    private DropsGrpcService dropsGrpcService;

    @BeforeEach
    void setUp() {
        reset(eventRepository, participantRepository, mongoTemplate);
    }

    @Test
    void claimAccessKeyWhenChallengeMatchesShouldAssignSingleWinner() {
        String eventId = "event-1";
        String userId = "user-1";
        when(participantRepository.existsByEventIdAndUserId(eventId, userId)).thenReturn(true);

        Event winner = activeEvent(eventId);
        winner.setWinnerUserId(userId);
        winner.setWinnerUsername("spectrum");
        winner.setFinishedAt(Instant.now().toEpochMilli());
        winner.setStatus("FINISHED");

        when(mongoTemplate.findAndModify(
                any(Query.class),
                any(UpdateDefinition.class),
                any(FindAndModifyOptions.class),
                eq(Event.class)))
                .thenReturn(winner);

        CapturingObserver<ClaimKeyResponse> observer = new CapturingObserver<>();
        dropsGrpcService.claimAccessKey(ClaimKeyRequest.newBuilder()
                .setEventId(eventId)
                .setUserId(userId)
                .setUsername("spectrum")
                .setChallengeCode("READY")
                .build(), observer);

        assertTrue(observer.value.getSuccess());
        assertEquals(userId, observer.value.getWinnerUserId());
        assertTrue(observer.completed);
    }

    @Test
    void claimAccessKeyWhenHundredUsersRaceShouldReturnOnlyOneWinner() throws InterruptedException {
        String eventId = "event-race";
        AtomicBoolean winnerAssigned = new AtomicBoolean(false);
        Queue<ClaimKeyResponse> responses = new ConcurrentLinkedQueue<>();

        when(participantRepository.existsByEventIdAndUserId(eq(eventId), anyString())).thenReturn(true);
        when(eventRepository.findById(eventId)).thenAnswer(invocation -> {
            Event event = activeEvent(eventId);
            event.setWinnerUserId("winner");
            event.setWinnerUsername("winner-name");
            event.setFinishedAt(Instant.now().toEpochMilli());
            event.setStatus("FINISHED");
            return Optional.of(event);
        });
        when(mongoTemplate.findAndModify(
                any(Query.class),
                any(UpdateDefinition.class),
                any(FindAndModifyOptions.class),
                eq(Event.class)))
                .thenAnswer(invocation -> {
                    if (!winnerAssigned.compareAndSet(false, true)) {
                        return null;
                    }

                    Event event = activeEvent(eventId);
                    event.setWinnerUserId("winner");
                    event.setWinnerUsername("winner-name");
                    event.setFinishedAt(Instant.now().toEpochMilli());
                    event.setStatus("FINISHED");
                    return event;
                });

        int attempts = 100;
        CountDownLatch latch = new CountDownLatch(attempts);
        var executor = Executors.newFixedThreadPool(20);
        for (int index = 0; index < attempts; index++) {
            int userNumber = index;
            executor.submit(() -> {
                dropsGrpcService.claimAccessKey(ClaimKeyRequest.newBuilder()
                        .setEventId(eventId)
                        .setUserId("user-" + userNumber)
                        .setUsername("user-" + userNumber)
                        .setChallengeCode("READY")
                        .build(), new StreamObserver<>() {
                    @Override
                    public void onNext(ClaimKeyResponse value) {
                        responses.add(value);
                    }

                    @Override
                    public void onError(Throwable throwable) {
                        latch.countDown();
                    }

                    @Override
                    public void onCompleted() {
                        latch.countDown();
                    }
                });
            });
        }

        assertTrue(latch.await(10, TimeUnit.SECONDS));
        executor.shutdownNow();

        assertEquals(attempts, responses.size());
        assertEquals(1, responses.stream().filter(ClaimKeyResponse::getSuccess).count());
    }

    @Test
    void joinEventWhenSlotAvailableShouldCreateParticipationAndDecrementInventory() {
        String eventId = "event-join";
        String userId = "user-1";
        when(participantRepository.existsByEventIdAndUserId(eventId, userId)).thenReturn(false);
        when(participantRepository.save(any(EventParticipant.class))).thenAnswer(invocation -> invocation.getArgument(0));

        Event updated = activeEvent(eventId);
        updated.setAvailableSlots(9);
        updated.setParticipantsCount(1);

        when(mongoTemplate.findAndModify(
                any(Query.class),
                any(UpdateDefinition.class),
                any(FindAndModifyOptions.class),
                eq(Event.class)))
                .thenReturn(updated);

        CapturingObserver<EventActionResponse> observer = new CapturingObserver<>();
        dropsGrpcService.joinEvent(JoinEventRequest.newBuilder()
                .setEventId(eventId)
                .setUserId(userId)
                .build(), observer);

        assertTrue(observer.value.getSuccess());
        verify(participantRepository).save(any(EventParticipant.class));
    }

    @Test
    void createEventWhenDatesAreInvalidShouldReturnError() {
        long now = Instant.now().toEpochMilli();
        CapturingObserver<EventActionResponse> observer = new CapturingObserver<>();

        dropsGrpcService.createEvent(CreateEventRequest.newBuilder()
                .setTitle("Invalid")
                .setGameTitle("Halo")
                .setPlatform("PC")
                .setStartAt(now + 2000)
                .setJoinDeadlineAt(now + 1000)
                .setRevealAt(now + 3000)
                .setEndAt(now + 4000)
                .setTotalSlots(10)
                .setPublicChallengeCode("READY")
                .build(), observer);

        assertFalse(observer.value.getSuccess());
        verify(eventRepository, never()).save(any());
    }

    @Test
    void getEventStatusWhenEventExistsShouldReturnDetails() {
        Event event = activeEvent("event-1");
        when(eventRepository.findById("event-1")).thenReturn(Optional.of(event));

        CapturingObserver<EventStatusResponse> observer = new CapturingObserver<>();
        dropsGrpcService.getEventStatus(GetEventRequest.newBuilder().setEventId("event-1").build(), observer);

        assertEquals("event-1", observer.value.getEventId());
        assertEquals("ACTIVE", observer.value.getStatus());
        assertEquals(10, observer.value.getTotalSlots());
    }

    private static Event activeEvent(String eventId) {
        long now = Instant.now().toEpochMilli();
        Event event = new Event();
        event.setId(eventId);
        event.setTitle("Launch Drop");
        event.setDescription("Reward");
        event.setGameTitle("Halo");
        event.setPlatform("PC");
        event.setImageUrl("https://example.test/halo.jpg");
        event.setStatus("ACTIVE");
        event.setStartAt(now - 1_000);
        event.setJoinDeadlineAt(now + 10_000);
        event.setRevealAt(now - 500);
        event.setEndAt(now + 20_000);
        event.setTotalSlots(10);
        event.setAvailableSlots(10);
        event.setPublicChallengeCode("READY");
        event.setRewardDeliveryStatus("PENDING");
        return event;
    }

    private static class CapturingObserver<T> implements StreamObserver<T> {
        private T value;
        private boolean completed;

        @Override
        public void onNext(T value) {
            this.value = value;
        }

        @Override
        public void onError(Throwable throwable) {
            fail(throwable);
        }

        @Override
        public void onCompleted() {
            completed = true;
        }
    }
}
