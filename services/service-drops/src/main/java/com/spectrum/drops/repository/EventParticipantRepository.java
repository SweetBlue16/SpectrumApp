package com.spectrum.drops.repository;

import com.spectrum.drops.model.EventParticipant;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface EventParticipantRepository extends MongoRepository<EventParticipant, String> {
    boolean existsByEventIdAndUserId(String eventId, String userId);
    void deleteByEventIdAndUserId(String eventId, String userId);
    List<EventParticipant> findByUserId(String userId);
}
