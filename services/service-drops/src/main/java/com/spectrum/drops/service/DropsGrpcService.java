package com.spectrum.drops.service;

import com.spectrum.drops.grpc.*;
import com.spectrum.drops.model.AccessKey;
import com.spectrum.drops.model.Event;
import com.spectrum.drops.repository.AccessKeyRepository;
import com.spectrum.drops.repository.EventRepository;
import io.grpc.stub.StreamObserver;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import net.devh.boot.grpc.server.service.GrpcService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.dao.DataAccessException;
import org.springframework.data.mongodb.core.FindAndModifyOptions;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.data.mongodb.core.query.Query;
import org.springframework.data.mongodb.core.query.Update;

import java.time.Instant;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.stream.Collectors;

@GrpcService
@RequiredArgsConstructor
@Slf4j
public class DropsGrpcService extends DropServiceGrpc.DropServiceImplBase {

    private static final Logger logger = LoggerFactory.getLogger(DropsGrpcService.class);
    private static final String STATUS_FIELD = "status";

    private final AccessKeyRepository accessKeyRepository;
    private final EventRepository eventRepository;
    private final MongoTemplate mongoTemplate;

    @Override
    public void getEventStatus(GetEventRequest request, StreamObserver<EventStatusResponse> responseObserver) {
        try {
            if (request.getEventId().isBlank()) {
                throw new IllegalArgumentException("EventId is required.");
            }

            Optional<Event> eventOpt = eventRepository.findById(request.getEventId());
            if (eventOpt.isPresent()) {
                Event event = eventOpt.get();
                responseObserver.onNext(EventStatusResponse.newBuilder()
                        .setEventId(event.getId())
                        .setKeysAvailable(event.getKeysAvailable())
                        .setKeysTotal(event.getKeysTotal())
                        .setStatus(event.getStatus())
                        .setEndDate(event.getEndDate())
                        .build());
            } else {
                responseObserver.onNext(EventStatusResponse.newBuilder()
                        .setEventId(request.getEventId())
                        .setStatus("NOT_FOUND")
                        .build());
            }
            responseObserver.onCompleted();
        } catch (Exception e) {
            logger.error("Error fetching event status", e);
            responseObserver.onCompleted();
        }
    }

    @Override
    public void claimAccessKey(ClaimKeyRequest request, StreamObserver<ClaimKeyResponse> responseObserver) {
        try {
            validateClaimKeyRequest(request);

            if (accessKeyRepository.existsByEventIdAndClaimedByUserId(request.getEventId(), request.getUserId())) {
                throw new IllegalStateException("You have already claimed a key for this event.");
            }

            getValidEventForClaiming(request.getEventId());

            Query query = new Query(Criteria.where("eventId").is(request.getEventId())
                    .and(STATUS_FIELD).is("AVAILABLE"));

            Update update = new Update()
                    .set(STATUS_FIELD, "CLAIMED")
                    .set("claimedByUserId", request.getUserId())
                    .set("claimedAt", Instant.now().toEpochMilli());

            AccessKey wonKey = mongoTemplate.findAndModify(
                    query,
                    update,
                    FindAndModifyOptions.options().returnNew(true),
                    AccessKey.class
            );

            if (wonKey == null) {
                updateEventToDepleted(request.getEventId());
                throw new IllegalStateException("All keys have been claimed. Better luck next time!");
            }

            decrementEventKeys(request.getEventId());

            responseObserver.onNext(ClaimKeyResponse.newBuilder()
                    .setSuccess(true)
                    .setAccessKeyCode(wonKey.getKeyCode())
                    .setClaimedAt(wonKey.getClaimedAt())
                    .build());
            responseObserver.onCompleted();
        } catch (IllegalArgumentException e) {
            logger.warn("Validation error claiming key: {}", e.getMessage());
            sendClaimError(responseObserver);
        } catch (IllegalStateException e) {
            logger.info("Business rule applied for user {}: {}", request.getUserId(), e.getMessage());
            sendClaimError(responseObserver);
        } catch (DataAccessException e) {
            logger.error("Database error claiming key for user {}", request.getUserId(), e);
            sendClaimError(responseObserver);
        } catch (Exception e) {
            logger.error("Unexpected error in claimAccessKey", e);
            sendClaimError(responseObserver);
        }
    }

    @Override
    public void getWonKeys(WonKeysRequest request, StreamObserver<WonKeysResponse> responseObserver) {
        try {
            if (request.getUserId().isBlank()) {
                throw new IllegalArgumentException("UserId is required.");
            }

            List<AccessKey> userKeys = accessKeyRepository.findByClaimedByUserId(request.getUserId());

            List<String> eventIds = userKeys.stream().map(AccessKey::getEventId).distinct().toList();
            Map<String, String> eventTitles = eventRepository.findAllById(eventIds).stream()
                    .collect(Collectors.toMap(Event::getId, Event::getGameTitle));

            WonKeysResponse.Builder responseBuilder = WonKeysResponse.newBuilder();

            for (AccessKey key : userKeys) {
                String title = eventTitles.getOrDefault(key.getEventId(), "Unknown Title");
                WonKey wonKey = WonKey.newBuilder()
                        .setEventId(key.getEventId())
                        .setGameTitle(title)
                        .setAccessKeyCode(key.getKeyCode())
                        .setClaimedAt(key.getClaimedAt())
                        .build();
                responseBuilder.addWonKeys(wonKey);
            }

            responseObserver.onNext(responseBuilder.build());
            responseObserver.onCompleted();
        } catch (Exception e) {
            logger.error("Error fetching won keys for user {}", request.getUserId(), e);
            responseObserver.onNext(WonKeysResponse.getDefaultInstance());
            responseObserver.onCompleted();
        }
    }

    @Override
    public void createEvent(CreateEventRequest request, StreamObserver<EventActionResponse> responseObserver) {
        try {
            if (request.getAccessKeysList().isEmpty()) {
                throw new IllegalArgumentException("At least one access key is required to create an event.");
            }

            Event event = new Event();
            event.setGameTitle(request.getGameTitle());
            event.setCoverImageUrl(request.getCoverImageUrl());
            event.setEndDate(request.getEndDate());
            event.setKeysTotal(request.getAccessKeysCount());
            event.setKeysAvailable(request.getAccessKeysCount());
            event.setStatus("ACTIVE");

            Event savedEvent = eventRepository.save(event);

            List<AccessKey> keysToInsert = request.getAccessKeysList().stream().map(code -> {
                AccessKey key = new AccessKey();
                key.setEventId(savedEvent.getId());
                key.setKeyCode(code);
                key.setStatus("AVAILABLE");
                return key;
            }).toList();

            accessKeyRepository.saveAll(keysToInsert);

            responseObserver.onNext(EventActionResponse.newBuilder()
                    .setSuccess(true)
                    .setMessage("Event created successfully with " + savedEvent.getKeysTotal() + " keys.")
                    .setEventId(savedEvent.getId())
                    .build());
            responseObserver.onCompleted();
        } catch (Exception e) {
            logger.error("Error creating drop event", e);
            responseObserver.onNext(EventActionResponse.newBuilder()
                    .setSuccess(false)
                    .setMessage("Failed to create event: " + e.getMessage())
                    .build());
            responseObserver.onCompleted();
        }
    }

    @Override
    public void updateEvent(UpdateEventRequest request, StreamObserver<EventActionResponse> responseObserver) {
        try {
            Optional<Event> eventOpt = eventRepository.findById(request.getEventId());
            if (eventOpt.isPresent()) {
                Event event = eventOpt.get();
                event.setGameTitle(request.getGameTitle());
                event.setCoverImageUrl(request.getCoverImageUrl());
                event.setEndDate(request.getEndDate());
                event.setStatus(request.getStatus());

                eventRepository.save(event);

                responseObserver.onNext(EventActionResponse.newBuilder()
                        .setSuccess(true)
                        .setMessage("Event updated successfully.")
                        .setEventId(event.getId())
                        .build());
            } else {
                throw new IllegalArgumentException("Event not found.");
            }
            responseObserver.onCompleted();
        } catch (Exception e) {
            logger.error("Error updating drop event", e);
            responseObserver.onNext(EventActionResponse.newBuilder()
                    .setSuccess(false)
                    .setMessage(e.getMessage())
                    .build());
            responseObserver.onCompleted();
        }
    }

    private void validateClaimKeyRequest(ClaimKeyRequest request) {
        if (request.getUserId().isBlank() || request.getEventId().isBlank()) {
            throw new IllegalArgumentException("UserId and EventId are mandatory.");
        }
    }

    private void getValidEventForClaiming(String eventId) {
        Optional<Event> eventOpt = eventRepository.findById(eventId);
        if (eventOpt.isEmpty()) {
            throw new IllegalArgumentException("Event not found.");
        }

        Event event = eventOpt.get();
        if (!"ACTIVE".equals(event.getStatus())) {
            throw new IllegalStateException("This event is no longer active.");
        }
        if (Instant.now().toEpochMilli() > event.getEndDate()) {
            throw new IllegalStateException("This event has expired.");
        }
    }

    private void decrementEventKeys(String eventId) {
        Query query = new Query(Criteria.where("id").is(eventId));
        Update update = new Update().inc("keysAvailable", -1);
        mongoTemplate.updateFirst(query, update, Event.class);
    }

    private void updateEventToDepleted(String eventId) {
        Query query = new Query(Criteria.where("id").is(eventId).and("keysAvailable").lte(0));
        Update update = new Update().set(STATUS_FIELD, "DEPLETED");
        mongoTemplate.updateFirst(query, update, Event.class);
    }

    private void sendClaimError(StreamObserver<ClaimKeyResponse> observer) {
        observer.onNext(ClaimKeyResponse.newBuilder()
                .setSuccess(false)
                .setAccessKeyCode("")
                .build());
        observer.onCompleted();
    }
}
