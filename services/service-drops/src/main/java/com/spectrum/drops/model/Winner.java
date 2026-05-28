package com.spectrum.drops.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class Winner {
    private String userId;
    private String username;
    private String rewardCode;
    private Long claimedAt;
    private String deliveryStatus;
}
