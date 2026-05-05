package com.spectrum.social.service;

import com.spectrum.social.grpc.CastVoteRequest;
import com.spectrum.social.grpc.VoteResponse;
import com.spectrum.social.grpc.VoteServiceGrpc;
import io.grpc.Status;
import io.grpc.stub.StreamObserver;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import net.devh.boot.grpc.server.service.GrpcService;

@GrpcService
@RequiredArgsConstructor
@Slf4j
public class VoteGrpcService extends VoteServiceGrpc.VoteServiceImplBase {

    private static final String REVIEW_TARGET_TYPE = "REVIEW";

    private final VoteApplicationService voteApplicationService;

    @Override
    public void castVote(CastVoteRequest request, StreamObserver<VoteResponse> responseObserver) {
        if (hasMissingRequiredFields(request)) {
            responseObserver.onError(Status.INVALID_ARGUMENT
                    .withDescription("userId, targetId and targetType are required.")
                    .asRuntimeException());
            return;
        }

        if (!REVIEW_TARGET_TYPE.equals(request.getTargetType())) {
            responseObserver.onError(Status.INVALID_ARGUMENT
                    .withDescription("Only REVIEW votes are supported.")
                    .asRuntimeException());
            return;
        }

        try {
            VoteApplicationService.VoteCounts counts = voteApplicationService.castVote(
                    request.getUserId(),
                    request.getTargetId(),
                    request.getTargetType(),
                    request.getIsPositive()
            );

            log.info(
                    "Vote processed userId={} targetId={} targetType={} isPositive={} updatedLikes={} updatedDislikes={}",
                    request.getUserId(),
                    request.getTargetId(),
                    request.getTargetType(),
                    request.getIsPositive(),
                    counts.updatedLikes(),
                    counts.updatedDislikes()
            );

            responseObserver.onNext(VoteResponse.newBuilder()
                    .setSuccess(true)
                    .setUpdatedLikes(counts.updatedLikes())
                    .setUpdatedDislikes(counts.updatedDislikes())
                    .build());
            responseObserver.onCompleted();
        } catch (RuntimeException ex) {
            log.error(
                    "Failed to process vote userId={} targetId={} targetType={} isPositive={}",
                    request.getUserId(),
                    request.getTargetId(),
                    request.getTargetType(),
                    request.getIsPositive(),
                    ex
            );
            responseObserver.onError(Status.INTERNAL
                    .withDescription("Vote could not be processed.")
                    .withCause(ex)
                    .asRuntimeException());
        }
    }

    private static boolean hasMissingRequiredFields(CastVoteRequest request) {
        return request.getUserId().isBlank()
                || request.getTargetId().isBlank()
                || request.getTargetType().isBlank();
    }
}
