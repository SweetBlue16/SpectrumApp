package com.spectrum.social.model;

import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.AllArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.index.CompoundIndex;
import org.springframework.data.mongodb.core.mapping.Document;
import org.springframework.data.mongodb.core.mapping.Field;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Document(collection = "votes")
@CompoundIndex(
        name = "ux_votes_user_target_type",
        def = "{'userId': 1, 'targetId': 1, 'targetType': 1}",
        unique = true
)
public class Vote {
    @Id
    private String id;
    private String userId;
    private String targetId;
    private String targetType;

    @Field("isPositive")
    private boolean positive;
}
