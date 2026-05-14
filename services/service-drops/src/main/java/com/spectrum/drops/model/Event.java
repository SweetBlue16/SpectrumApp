package com.spectrum.drops.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Document(collection = "events")
public class Event {
    @Id
    private String id;
    private String gameTitle;
    private String coverImageUrl;
    private int keysTotal;
    private int keysAvailable;
    private String status;
    private long startDate;
    private long endDate;
}
