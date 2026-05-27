package com.spectrum.drops.service;

import com.spectrum.drops.grpc.*;
import com.spectrum.drops.model.Event;
import com.spectrum.drops.model.EventParticipant;
import com.spectrum.drops.repository.EventParticipantRepository;
import com.spectrum.drops.repository.EventRepository;
import io.grpc.stub.StreamObserver;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import net.devh.boot.grpc.server.service.GrpcService;
import org.springframework.dao.DataAccessException;
import org.springframework.dao.DuplicateKeyException;
import org.springframework.data.domain.Sort;
import org.springframework.data.mongodb.core.FindAndModifyOptions;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.data.mongodb.core.query.Query;
import org.springframework.data.mongodb.core.query.Update;

import java.time.Instant;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

@GrpcService
@RequiredArgsConstructor
@Slf4j
public class DropsGrpcService extends DropServiceGrpc.DropServiceImplBase {

    private static final String DRAFT = "DRAFT";
    private static final String SCHEDULED = "SCHEDULED";
    private static final String ACTIVE = "ACTIVE";
    private static final String JOIN_CLOSED = "JOIN_CLOSED";
    private static final String REVEALED = "REVEALED";
    private static final String FINISHED = "FINISHED";
    private static final String CANCELLED = "CANCELLED";
    private static final String REWARD_PENDING = "PENDING";
    private static final String REWARD_SENT = "SENT";

    private static final String STATUS_FIELD = "status";
    private static final String EVENT_ID_FIELD = "_id";

    private final EventRepository eventRepository;
    private final EventParticipantRepository participantRepository;
    private final MongoTemplate mongoTemplate;

    @Override
    public void createEvent(CreateEventRequest request, StreamObserver<EventActionResponse> responseObserver) {
        try {
            validateEventPayload(
                    request.getTitle(),
                    request.getGameTitle(),
                    request.getPlatform(),
                    request.getStartAt(),
                    request.getJoinDeadlineAt(),
                    request.getRevealAt(),
                    request.getEndAt(),
                    request.getTotalSlots(),
                    request.getPublicChallengeCode()
            );

            long now = Instant.now().toEpochMilli();
            Event event = Event.builder()
                    .title(request.getTitle().trim())
                    .description(request.getDescription().trim())
                    .imageUrl(request.getImageUrl().trim())
                    .gameTitle(request.getGameTitle().trim())
                    .rawgGameId(request.getRawgGameId())
                    .platform(request.getPlatform().trim())
                    .startAt(request.getStartAt())
                    .joinDeadlineAt(request.getJoinDeadlineAt())
                    .revealAt(request.getRevealAt())
                    .endAt(request.getEndAt())
                    .totalSlots(request.getTotalSlots())
                    .availableSlots(request.getTotalSlots())
                    .keysTotal(request.getTotalSlots())
                    .keysAvailable(request.getTotalSlots())
                    .publicChallengeCode(normalizeChallenge(request.getPublicChallengeCode()))
                    .createdByAdminId(request.getCreatedByAdminId())
                    .status(request.getPublishNow() ? initialPublishedStatus(request.getStartAt(), now) : DRAFT)
                    .rewardDeliveryStatus(REWARD_PENDING)
                    .participantsCount(0)
                    .build();

            Event savedEvent = eventRepository.save(event);

            responseObserver.onNext(EventActionResponse.newBuilder()
                    .setSuccess(true)
                    .setMessage("Event created successfully.")
                    .setEventId(savedEvent.getId())
                    .build());
            responseObserver.onCompleted();
        } catch (IllegalArgumentException e) {
            sendActionError(responseObserver, e.getMessage());
        } catch (Exception e) {
            log.error("Error creating giveaway event", e);
            sendActionError(responseObserver, "Failed to create event.");
        }
    }

    @Override
    public void updateEvent(UpdateEventRequest request, StreamObserver<EventActionResponse> responseObserver) {
        try {
            validateEventId(request.getEventId());
            validateEventPayload(
                    request.getTitle(),
                    request.getGameTitle(),
                    request.getPlatform(),
                    request.getStartAt(),
                    request.getJoinDeadlineAt(),
                    request.getRevealAt(),
                    request.getEndAt(),
                    request.getTotalSlots(),
                    request.getPublicChallengeCode()
            );

            Event event = eventRepository.findById(request.getEventId())
                    .orElseThrow(() -> new IllegalArgumentException("Event not found."));

            if (event.getWinnerUserId() != null && !event.getWinnerUserId().isBlank()) {
                throw new IllegalStateException("Finished events with a winner cannot be edited.");
            }

            if (event.getParticipantsCount() > request.getTotalSlots()) {
                throw new IllegalArgumentException("Total slots cannot be lower than current participants.");
            }

            event.setTitle(request.getTitle().trim());
            event.setDescription(request.getDescription().trim());
            event.setImageUrl(request.getImageUrl().trim());
            event.setGameTitle(request.getGameTitle().trim());
            event.setRawgGameId(request.getRawgGameId());
            event.setPlatform(request.getPlatform().trim());
            event.setStartAt(request.getStartAt());
            event.setJoinDeadlineAt(request.getJoinDeadlineAt());
            event.setRevealAt(request.getRevealAt());
            event.setEndAt(request.getEndAt());
            event.setTotalSlots(request.getTotalSlots());
            event.setKeysTotal(request.getTotalSlots());
            event.setAvailableSlots(request.getTotalSlots() - event.getParticipantsCount());
            event.setKeysAvailable(event.getAvailableSlots());
            event.setPublicChallengeCode(normalizeChallenge(request.getPublicChallengeCode()));
            if (!request.getStatus().isBlank()) {
                event.setStatus(request.getStatus());
            }

            eventRepository.save(event);
            sendActionSuccess(responseObserver, event.getId(), "Event updated successfully.");
        } catch (IllegalArgumentException | IllegalStateException e) {
            sendActionError(responseObserver, e.getMessage());
        } catch (Exception e) {
            log.error("Error updating giveaway event {}", request.getEventId(), e);
            sendActionError(responseObserver, "Failed to update event.");
        }
    }

    @Override
    public void publishEvent(PublishEventRequest request, StreamObserver<EventActionResponse> responseObserver) {
        try {
            validateEventId(request.getEventId());
            Event event = eventRepository.findById(request.getEventId())
                    .orElseThrow(() -> new IllegalArgumentException("Event not found."));

            if (FINISHED.equals(event.getStatus()) || CANCELLED.equals(event.getStatus())) {
                throw new IllegalStateException("Finished or cancelled events cannot be published.");
            }

            event.setStatus(initialPublishedStatus(event.getStartAt(), Instant.now().toEpochMilli()));
            eventRepository.save(event);
            sendActionSuccess(responseObserver, event.getId(), "Event published successfully.");
        } catch (IllegalArgumentException | IllegalStateException e) {
            sendActionError(responseObserver, e.getMessage());
        } catch (Exception e) {
            log.error("Error publishing event {}", request.getEventId(), e);
            sendActionError(responseObserver, "Failed to publish event.");
        }
    }

    @Override
    public void finishEvent(FinishEventRequest request, StreamObserver<EventActionResponse> responseObserver) {
        try {
            validateEventId(request.getEventId());
            Event event = eventRepository.findById(request.getEventId())
                    .orElseThrow(() -> new IllegalArgumentException("Event not found."));

            if (FINISHED.equals(event.getStatus()) || CANCELLED.equals(event.getStatus())) {
                sendActionSuccess(responseObserver, event.getId(), "Event already closed.");
                return;
            }

            boolean withoutWinner = event.getWinnerUserId() == null || event.getWinnerUserId().isBlank();
            event.setStatus(withoutWinner && request.getCancelIfWithoutWinner() ? CANCELLED : FINISHED);
            event.setFinishedAt(Instant.now().toEpochMilli());
            eventRepository.save(event);

            sendActionSuccess(responseObserver, event.getId(), "Event closed successfully.");
        } catch (IllegalArgumentException e) {
            sendActionError(responseObserver, e.getMessage());
        } catch (Exception e) {
            log.error("Error finishing event {}", request.getEventId(), e);
            sendActionError(responseObserver, "Failed to finish event.");
        }
    }

    @Override
    public void joinEvent(JoinEventRequest request, StreamObserver<EventActionResponse> responseObserver) {
        EventParticipant insertedParticipant = null;
        try {
            validateEventId(request.getEventId());
            if (request.getUserId().isBlank()) {
                throw new IllegalArgumentException("UserId is required.");
            }

            if (participantRepository.existsByEventIdAndUserId(request.getEventId(), request.getUserId())) {
                throw new IllegalStateException("duplicateParticipation");
            }

            insertedParticipant = EventParticipant.builder()
                    .eventId(request.getEventId())
                    .userId(request.getUserId())
                    .joinedAt(Instant.now().toEpochMilli())
                    .build();
            participantRepository.save(insertedParticipant);

            long now = Instant.now().toEpochMilli();
            Query query = new Query(Criteria.where(EVENT_ID_FIELD).is(request.getEventId())
                    .and(STATUS_FIELD).in(SCHEDULED, ACTIVE)
                    .and("startAt").lte(now)
                    .and("joinDeadlineAt").gte(now)
                    .and("availableSlots").gt(0));

            Update update = new Update()
                    .inc("availableSlots", -1)
                    .inc("keysAvailable", -1)
                    .inc("participantsCount", 1);

            Event updated = mongoTemplate.findAndModify(
                    query,
                    update,
                    FindAndModifyOptions.options().returnNew(true),
                    Event.class
            );

            if (updated == null) {
                participantRepository.deleteByEventIdAndUserId(request.getEventId(), request.getUserId());
                throw new IllegalStateException("Event is not accepting participants.");
            }

            sendActionSuccess(responseObserver, updated.getId(), "User joined event.");
        } catch (DuplicateKeyException e) {
            sendActionError(responseObserver, "duplicateParticipation");
        } catch (IllegalArgumentException | IllegalStateException e) {
            rollbackParticipant(insertedParticipant);
            sendActionError(responseObserver, e.getMessage());
        } catch (DataAccessException e) {
            rollbackParticipant(insertedParticipant);
            log.error("Database error joining event {}", request.getEventId(), e);
            sendActionError(responseObserver, "Could not join event.");
        } catch (Exception e) {
            rollbackParticipant(insertedParticipant);
            log.error("Unexpected error joining event {}", request.getEventId(), e);
            sendActionError(responseObserver, "Could not join event.");
        }
    }

    @Override
    public void claimAccessKey(ClaimKeyRequest request, StreamObserver<ClaimKeyResponse> responseObserver) {
        try {
            validateEventId(request.getEventId());
            if (request.getUserId().isBlank() || request.getUsername().isBlank()) {
                throw new IllegalArgumentException("UserId and username are required.");
            }

            if (request.getChallengeCode().length() > 50) {
                throw new IllegalArgumentException("Challenge code is too long.");
            }

            if (!participantRepository.existsByEventIdAndUserId(request.getEventId(), request.getUserId())) {
                throw new IllegalStateException("User must join before claiming.");
            }

            long now = Instant.now().toEpochMilli();
            Query query = new Query(Criteria.where(EVENT_ID_FIELD).is(request.getEventId())
                    .and(STATUS_FIELD).in(SCHEDULED, ACTIVE, JOIN_CLOSED, REVEALED)
                    .and("winnerUserId").is(null)
                    .and("publicChallengeCode").is(normalizeChallenge(request.getChallengeCode()))
                    .and("startAt").lte(now)
                    .and("revealAt").lte(now)
                    .and("endAt").gte(now));

            Update update = new Update()
                    .set("winnerUserId", request.getUserId())
                    .set("winnerUsername", request.getUsername())
                    .set("finishedAt", now)
                    .set(STATUS_FIELD, FINISHED);

            Event updated = mongoTemplate.findAndModify(
                    query,
                    update,
                    FindAndModifyOptions.options().returnNew(true),
                    Event.class
            );

            if (updated == null) {
                Optional<Event> event = eventRepository.findById(request.getEventId());
                ClaimKeyResponse.Builder response = ClaimKeyResponse.newBuilder()
                        .setSuccess(false)
                        .setAccessKeyCode("")
                        .setMessage("Challenge could not be claimed.");

                event.ifPresent(value -> {
                    if (value.getWinnerUserId() != null) {
                        response.setWinnerUserId(value.getWinnerUserId());
                    }
                    if (value.getWinnerUsername() != null) {
                        response.setWinnerUsername(value.getWinnerUsername());
                    }
                    if (value.getFinishedAt() != null) {
                        response.setClaimedAt(value.getFinishedAt());
                    }
                });

                responseObserver.onNext(response.build());
                responseObserver.onCompleted();
                return;
            }

            responseObserver.onNext(ClaimKeyResponse.newBuilder()
                    .setSuccess(true)
                    .setAccessKeyCode("")
                    .setWinnerUserId(updated.getWinnerUserId())
                    .setWinnerUsername(updated.getWinnerUsername())
                    .setClaimedAt(updated.getFinishedAt() == null ? now : updated.getFinishedAt())
                    .setMessage("Winner assigned.")
                    .build());
            responseObserver.onCompleted();
        } catch (IllegalArgumentException | IllegalStateException e) {
            log.info("Claim rejected for event {}: {}", request.getEventId(), e.getMessage());
            responseObserver.onNext(ClaimKeyResponse.newBuilder()
                    .setSuccess(false)
                    .setAccessKeyCode("")
                    .setMessage(e.getMessage())
                    .build());
            responseObserver.onCompleted();
        } catch (Exception e) {
            log.error("Unexpected error in claimAccessKey for event {}", request.getEventId(), e);
            responseObserver.onNext(ClaimKeyResponse.newBuilder()
                    .setSuccess(false)
                    .setAccessKeyCode("")
                    .setMessage("Could not claim event.")
                    .build());
            responseObserver.onCompleted();
        }
    }

    @Override
    public void getEventStatus(GetEventRequest request, StreamObserver<EventStatusResponse> responseObserver) {
        try {
            validateEventId(request.getEventId());
            Event event = eventRepository.findById(request.getEventId())
                    .orElseThrow(() -> new IllegalArgumentException("Event not found."));

            responseObserver.onNext(toResponse(event));
            responseObserver.onCompleted();
        } catch (Exception e) {
            log.warn("Error fetching event status {}: {}", request.getEventId(), e.getMessage());
            responseObserver.onNext(EventStatusResponse.newBuilder()
                    .setEventId(request.getEventId())
                    .setStatus("NOT_FOUND")
                    .build());
            responseObserver.onCompleted();
        }
    }

    @Override
    public void listEvents(ListEventsRequest request, StreamObserver<EventListResponse> responseObserver) {
        try {
            int page = Math.max(1, request.getPage());
            int pageSize = Math.min(Math.max(1, request.getPageSize()), 50);
            Query query = new Query(buildScopeCriteria(request.getScope(), request.getIncludeDrafts()));
            long total = mongoTemplate.count(query, Event.class);

            Sort sort = "UPCOMING".equalsIgnoreCase(request.getScope())
                    ? Sort.by(Sort.Direction.ASC, "startAt")
                    : Sort.by(Sort.Direction.DESC, "startAt");

            query.with(sort)
                    .skip((long) (page - 1) * pageSize)
                    .limit(pageSize);

            List<Event> events = mongoTemplate.find(query, Event.class);
            EventListResponse.Builder builder = EventListResponse.newBuilder()
                    .setTotalCount((int) total)
                    .setPage(page)
                    .setPageSize(pageSize);

            events.forEach(event -> builder.addEvents(toResponse(event)));
            responseObserver.onNext(builder.build());
            responseObserver.onCompleted();
        } catch (Exception e) {
            log.error("Error listing giveaway events", e);
            responseObserver.onNext(EventListResponse.newBuilder()
                    .setPage(Math.max(1, request.getPage()))
                    .setPageSize(Math.max(1, request.getPageSize()))
                    .build());
            responseObserver.onCompleted();
        }
    }

    @Override
    public void markRewardSent(MarkRewardSentRequest request, StreamObserver<EventActionResponse> responseObserver) {
        try {
            validateEventId(request.getEventId());
            Query query = new Query(Criteria.where(EVENT_ID_FIELD).is(request.getEventId())
                    .and(STATUS_FIELD).is(FINISHED)
                    .and("winnerUserId").ne(null));

            Update update = new Update()
                    .set("rewardSentAt", request.getRewardSentAt())
                    .set("rewardDeliveryStatus", REWARD_SENT);

            Event updated = mongoTemplate.findAndModify(
                    query,
                    update,
                    FindAndModifyOptions.options().returnNew(true),
                    Event.class
            );

            if (updated == null) {
                throw new IllegalStateException("Reward can only be marked sent for finished events with a winner.");
            }

            sendActionSuccess(responseObserver, updated.getId(), "Reward marked as sent.");
        } catch (IllegalArgumentException | IllegalStateException e) {
            sendActionError(responseObserver, e.getMessage());
        } catch (Exception e) {
            log.error("Error marking reward sent for event {}", request.getEventId(), e);
            sendActionError(responseObserver, "Could not mark reward as sent.");
        }
    }

    @Override
    public void getWonKeys(WonKeysRequest request, StreamObserver<WonKeysResponse> responseObserver) {
        try {
            if (request.getUserId().isBlank()) {
                throw new IllegalArgumentException("UserId is required.");
            }

            Query query = new Query(Criteria.where("winnerUserId").is(request.getUserId()))
                    .with(Sort.by(Sort.Direction.DESC, "finishedAt"));
            List<Event> events = mongoTemplate.find(query, Event.class);

            WonKeysResponse.Builder response = WonKeysResponse.newBuilder();
            for (Event event : events) {
                response.addWonKeys(WonKey.newBuilder()
                        .setEventId(event.getId())
                        .setGameTitle(nullToEmpty(event.getGameTitle()))
                        .setAccessKeyCode("")
                        .setClaimedAt(event.getFinishedAt() == null ? 0 : event.getFinishedAt())
                        .setRewardDeliveryStatus(nullToEmpty(event.getRewardDeliveryStatus()))
                        .build());
            }

            responseObserver.onNext(response.build());
            responseObserver.onCompleted();
        } catch (Exception e) {
            log.error("Error fetching won events for user {}", request.getUserId(), e);
            responseObserver.onNext(WonKeysResponse.getDefaultInstance());
            responseObserver.onCompleted();
        }
    }

    private Criteria buildScopeCriteria(String scope, boolean includeDrafts) {
        long now = Instant.now().toEpochMilli();
        List<Criteria> criteria = new ArrayList<>();
        if (!includeDrafts) {
            criteria.add(Criteria.where(STATUS_FIELD).nin(DRAFT, CANCELLED));
        }

        Criteria scopeCriteria = switch (scope == null ? "" : scope.toUpperCase()) {
            case "CURRENT" -> new Criteria().andOperator(
                    Criteria.where("startAt").lte(now),
                    Criteria.where("endAt").gte(now),
                    Criteria.where(STATUS_FIELD).nin(DRAFT, CANCELLED, FINISHED)
            );
            case "UPCOMING" -> Criteria.where("startAt").gt(now);
            case "PAST" -> new Criteria().orOperator(
                    Criteria.where(STATUS_FIELD).in(FINISHED, CANCELLED),
                    Criteria.where("endAt").lt(now)
            );
            default -> null;
        };
        if (scopeCriteria != null) {
            criteria.add(scopeCriteria);
        }

        return criteria.isEmpty()
                ? new Criteria()
                : new Criteria().andOperator(criteria.toArray(Criteria[]::new));
    }

    private EventStatusResponse toResponse(Event event) {
        String status = resolveDisplayStatus(event, Instant.now().toEpochMilli());
        return EventStatusResponse.newBuilder()
                .setEventId(nullToEmpty(event.getId()))
                .setKeysAvailable(event.getAvailableSlots())
                .setKeysTotal(event.getTotalSlots())
                .setStatus(status)
                .setEndDate(event.getEndAt())
                .setTitle(nullToEmpty(event.getTitle()))
                .setDescription(nullToEmpty(event.getDescription()))
                .setImageUrl(nullToEmpty(event.getImageUrl()))
                .setGameTitle(nullToEmpty(event.getGameTitle()))
                .setRawgGameId(event.getRawgGameId())
                .setPlatform(nullToEmpty(event.getPlatform()))
                .setStartAt(event.getStartAt())
                .setJoinDeadlineAt(event.getJoinDeadlineAt())
                .setRevealAt(event.getRevealAt())
                .setTotalSlots(event.getTotalSlots())
                .setAvailableSlots(event.getAvailableSlots())
                .setPublicChallengeCode(nullToEmpty(event.getPublicChallengeCode()))
                .setCreatedByAdminId(nullToEmpty(event.getCreatedByAdminId()))
                .setWinnerUserId(nullToEmpty(event.getWinnerUserId()))
                .setWinnerUsername(nullToEmpty(event.getWinnerUsername()))
                .setFinishedAt(event.getFinishedAt() == null ? 0 : event.getFinishedAt())
                .setRewardSentAt(event.getRewardSentAt() == null ? 0 : event.getRewardSentAt())
                .setRewardDeliveryStatus(nullToEmpty(event.getRewardDeliveryStatus()))
                .setParticipantsCount(event.getParticipantsCount())
                .build();
    }

    private String resolveDisplayStatus(Event event, long now) {
        if (DRAFT.equals(event.getStatus()) || CANCELLED.equals(event.getStatus()) || FINISHED.equals(event.getStatus())) {
            return event.getStatus();
        }

        if (now < event.getStartAt()) {
            return SCHEDULED;
        }

        if (now <= event.getJoinDeadlineAt()) {
            return ACTIVE;
        }

        if (now < event.getRevealAt()) {
            return JOIN_CLOSED;
        }

        if (now <= event.getEndAt()) {
            return REVEALED;
        }

        return FINISHED;
    }

    private String initialPublishedStatus(long startAt, long now) {
        return now >= startAt ? ACTIVE : SCHEDULED;
    }

    private void validateEventPayload(
            String title,
            String gameTitle,
            String platform,
            long startAt,
            long joinDeadlineAt,
            long revealAt,
            long endAt,
            int totalSlots,
            String publicChallengeCode
    ) {
        if (title == null || title.isBlank()) {
            throw new IllegalArgumentException("Title is required.");
        }
        if (gameTitle == null || gameTitle.isBlank()) {
            throw new IllegalArgumentException("Game title is required.");
        }
        if (platform == null || platform.isBlank()) {
            throw new IllegalArgumentException("Platform is required.");
        }
        if (totalSlots <= 0) {
            throw new IllegalArgumentException("Total slots must be greater than zero.");
        }
        if (publicChallengeCode == null || publicChallengeCode.isBlank() || publicChallengeCode.length() > 50) {
            throw new IllegalArgumentException("Public challenge code is required and must be at most 50 characters.");
        }
        if (!(startAt < joinDeadlineAt && joinDeadlineAt <= revealAt && revealAt < endAt)) {
            throw new IllegalArgumentException("Event dates are invalid.");
        }
    }

    private void validateEventId(String eventId) {
        if (eventId == null || eventId.isBlank()) {
            throw new IllegalArgumentException("EventId is required.");
        }
    }

    private String normalizeChallenge(String challengeCode) {
        return challengeCode == null ? "" : challengeCode.trim();
    }

    private String nullToEmpty(String value) {
        return value == null ? "" : value;
    }

    private void rollbackParticipant(EventParticipant insertedParticipant) {
        if (insertedParticipant != null) {
            participantRepository.deleteByEventIdAndUserId(
                    insertedParticipant.getEventId(),
                    insertedParticipant.getUserId()
            );
        }
    }

    private void sendActionSuccess(StreamObserver<EventActionResponse> observer, String eventId, String message) {
        observer.onNext(EventActionResponse.newBuilder()
                .setSuccess(true)
                .setMessage(message)
                .setEventId(eventId)
                .build());
        observer.onCompleted();
    }

    private void sendActionError(StreamObserver<EventActionResponse> observer, String message) {
        observer.onNext(EventActionResponse.newBuilder()
                .setSuccess(false)
                .setMessage(message)
                .build());
        observer.onCompleted();
    }
}
