package com.spectrum.drops.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class RewardCode {
    private String code;
    private boolean claimed;
    private String claimedByUserId;
    private String claimedByUsername;
    private Long claimedAt;
    private String deliveryStatus;
}
