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

import java.util.ArrayList;
import java.util.List;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Document(collection = "events")
@CompoundIndexes({
        @CompoundIndex(name = "ix_status_reveal_end", def = "{'status': 1, 'revealAt': 1, 'endAt': 1}"),
        @CompoundIndex(name = "ix_winners_user", def = "{'winners.userId': 1}"),
        @CompoundIndex(name = "ix_reward_claim_user", def = "{'rewardCodes.claimedByUserId': 1}")
})
public class Event {
    @Id
    private String id;
    private String title;
    private String description;
    private String imageUrl;
    private String gameTitle;
    private int rawgGameId;
    private String platform;
    private int keysTotal;
    private int keysAvailable;
    private int totalSlots;
    private int availableSlots;
    @Indexed
    private String status;
    @Indexed
    private long startAt;
    private long joinDeadlineAt;
    private long revealAt;
    private long endAt;
    private String publicChallengeCode;
    private String createdByAdminId;
    @Indexed
    private String winnerUserId;
    private String winnerUsername;
    private Long finishedAt;
    private Long rewardSentAt;
    private String rewardDeliveryStatus;
    private int participantsCount;
    @Builder.Default
    private List<RewardCode> rewardCodes = new ArrayList<>();
    @Builder.Default
    private List<Winner> winners = new ArrayList<>();
}
