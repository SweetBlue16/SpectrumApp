package com.spectrum.drops.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.index.CompoundIndex;
import org.springframework.data.mongodb.core.index.CompoundIndexes;
import org.springframework.data.mongodb.core.index.Indexed;
import org.springframework.data.mongodb.core.mapping.Document;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Document(collection = "event_participants")
@CompoundIndexes({
        @CompoundIndex(name = "ux_event_user", def = "{'eventId': 1, 'userId': 1}", unique = true)
})
public class EventParticipant {
    @Id
    private String id;

    @Indexed
    private String eventId;

    @Indexed
    private String userId;

    private long joinedAt;
}
