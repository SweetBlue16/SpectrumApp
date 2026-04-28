package com.spectrum.drops.model;

import lombok.Builder;
import lombok.Data;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;

import java.time.Instant;

@Data
@Builder
@Document(collection = "access_keys")
public class AccessKey {
    @Id
    private String id;
    private String userId;
    private String eventId;
    private String gameTitle;
    private String accessKeyCode;
    private Instant claimedAt;
}
