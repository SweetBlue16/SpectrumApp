package com.spectrum.drops.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.index.Indexed;
import org.springframework.data.mongodb.core.mapping.Document;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Document(collection = "access_keys")
public class AccessKey {
    @Id
    private String id;
    @Indexed
    private String eventId;
    @Indexed
    private String claimedByUserId;
    private String keyCode;
    private String status;
    private long claimedAt;
}
