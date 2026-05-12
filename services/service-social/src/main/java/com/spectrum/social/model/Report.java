package com.spectrum.social.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;

import java.time.Instant;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Document(collection = "reports")
public class Report {
    @Id
    private String id;
    private String reporterId;
    private String targetId;
    private String targetType;
    private String reason;
    private String status;
    private Instant reportedAt;
    private String moderatorId;
    private String resolutionNotes;
    private long resolvedAt;
}
