package com.spectrum.drops.service;

import com.spectrum.drops.grpc.ClaimKeyRequest;
import com.spectrum.drops.grpc.ClaimKeyResponse;
import com.spectrum.drops.grpc.DropServiceGrpc;
import com.spectrum.drops.grpc.EventStatusResponse;
import com.spectrum.drops.grpc.GetEventRequest;
import com.spectrum.drops.grpc.WonKey;
import com.spectrum.drops.grpc.WonKeysRequest;
import com.spectrum.drops.grpc.WonKeysResponse;
import com.spectrum.drops.repository.AccessKeyRepository;
import io.grpc.Status;
import io.grpc.stub.StreamObserver;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import net.devh.boot.grpc.server.service.GrpcService;

import java.util.stream.Collectors;

@GrpcService
@RequiredArgsConstructor
@Slf4j
public class DropsGrpcService extends DropServiceGrpc.DropServiceImplBase {

    private final AccessKeyRepository accessKeyRepository;

    @Override
    public void claimAccessKey(ClaimKeyRequest request, StreamObserver<ClaimKeyResponse> responseObserver) {
        log.info("Attempting to claim key for user: {} in event: {}", request.getUserId(), request.getEventId());

        // TODO: Implementar lógica de inventario y exclusión mutua
        com.spectrum.drops.grpc.ClaimKeyResponse response = com.spectrum.drops.grpc.ClaimKeyResponse.newBuilder()
                .setSuccess(false)
                .build();

        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }

    @Override
    public void getEventStatus(GetEventRequest request, StreamObserver<EventStatusResponse> responseObserver) {
        log.info("Checking status for event: {}", request.getEventId());

        EventStatusResponse response = EventStatusResponse.newBuilder()
                .setEventId(request.getEventId())
                .setKeysAvailable(571)
                .setKeysTotal(10000)
                .setStatus("ACTIVO")
                .setEndDate(1714000000L)
                .build();
        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }

    @Override
    public void getWonKeys(WonKeysRequest request, StreamObserver<WonKeysResponse> responseObserver) {
        log.info("Fetching won keys for user: {}", request.getUserId());

        try {
            var keysFromDb = accessKeyRepository.findByUserId(request.getUserId());

            var grpcKeys = keysFromDb.stream()
                    .map(k -> WonKey.newBuilder()
                            .setEventId(k.getEventId())
                            .setGameTitle(k.getGameTitle())
                            .setAccessKeyCode(k.getAccessKeyCode())
                            .setClaimedAt(k.getClaimedAt().getEpochSecond())
                            .build())
                    .collect(Collectors.toList());

            WonKeysResponse response = WonKeysResponse.newBuilder()
                    .addAllWonKeys(grpcKeys)
                    .build();

            responseObserver.onNext(response);
            responseObserver.onCompleted();
        } catch (Exception e) {
            log.error("Error fetching won keys", e);
            responseObserver.onError(Status.INTERNAL
                    .withDescription("Error accessing MongoDB")
                    .asRuntimeException());
        }
    }
}
