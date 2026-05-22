package com.spectrum.social.service;

import com.spectrum.social.grpc.CommentServiceGrpc;
import com.spectrum.social.grpc.PublishCommentRequest;
import com.spectrum.social.grpc.CommentResponse;
import com.spectrum.social.grpc.GetCommentsRequest;
import com.spectrum.social.grpc.DeleteCommentRequest;
import com.spectrum.social.grpc.DeleteResponse;
import com.spectrum.social.model.Comment;
import com.spectrum.social.repository.CommentRepository;
import io.grpc.Status;
import io.grpc.stub.StreamObserver;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import net.devh.boot.grpc.server.service.GrpcService;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;

import java.time.Instant;

@GrpcService
@RequiredArgsConstructor
@Slf4j
public class CommentGrpcService extends CommentServiceGrpc.CommentServiceImplBase {

    private static final int PAGE_SIZE = 20;
    private static final int MAX_CONTENT_LENGTH = 500;
    private static final String ADMIN_ROLE = "ADMIN";

    private final CommentRepository commentRepository;

    @Override
    public void publishComment(PublishCommentRequest request, StreamObserver<CommentResponse> responseObserver) {
        if (hasMissingPublishFields(request)) {
            responseObserver.onError(Status.INVALID_ARGUMENT
                    .withDescription("userId, reviewId and content are required.")
                    .asRuntimeException());
            return;
        }

        String content = request.getContent().trim();
        if (content.length() > MAX_CONTENT_LENGTH) {
            responseObserver.onError(Status.INVALID_ARGUMENT
                    .withDescription("El comentario no puede superar los 500 caracteres.")
                    .asRuntimeException());
            return;
        }

        Comment comment = Comment.builder()
                .userId(request.getUserId())
                .reviewId(request.getReviewId())
                .content(content)
                .publishedAt(Instant.now())
                .build();

        Comment savedComment = commentRepository.save(comment);

        log.info("Published comment {} from user {} on review {}", savedComment.getId(), request.getUserId(), request.getReviewId());
        responseObserver.onNext(toResponse(savedComment));
        responseObserver.onCompleted();
    }

    @Override
    public void getCommentsByReview(GetCommentsRequest request, StreamObserver<CommentResponse> responseObserver) {
        if (request.getReviewId().isBlank()) {
            responseObserver.onError(Status.INVALID_ARGUMENT
                    .withDescription("reviewId is required.")
                    .asRuntimeException());
            return;
        }

        int page = Math.max(request.getPage(), 1) - 1;
        PageRequest pageable = PageRequest.of(page, PAGE_SIZE, Sort.by(Sort.Direction.ASC, "publishedAt"));

        commentRepository.findByReviewId(request.getReviewId(), pageable)
                .forEach(comment -> responseObserver.onNext(toResponse(comment)));

        log.info("Fetched comments for review {}, page {}", request.getReviewId(), request.getPage());
        responseObserver.onCompleted();
    }

    @Override
    public void deleteComment(DeleteCommentRequest request, StreamObserver<DeleteResponse> responseObserver) {
        if (request.getCommentId().isBlank() || request.getRequesterId().isBlank()) {
            responseObserver.onError(Status.INVALID_ARGUMENT
                    .withDescription("commentId and requesterId are required.")
                    .asRuntimeException());
            return;
        }

        var optionalComment = commentRepository.findById(request.getCommentId());
        if (optionalComment.isEmpty()) {
            responseObserver.onError(Status.NOT_FOUND
                    .withDescription("El comentario solicitado no existe.")
                    .asRuntimeException());
            return;
        }

        Comment comment = optionalComment.get();
        boolean isOwner = comment.getUserId().equals(request.getRequesterId());
        boolean isAdmin = ADMIN_ROLE.equals(request.getRequesterRole());

        if (!isOwner && !isAdmin) {
            responseObserver.onError(Status.PERMISSION_DENIED
                    .withDescription("No tienes permisos para eliminar este comentario.")
                    .asRuntimeException());
            return;
        }

        commentRepository.delete(comment);
        log.info("Deleted comment {} requested by {}", request.getCommentId(), request.getRequesterId());
        responseObserver.onNext(DeleteResponse.newBuilder().setSuccess(true).build());
        responseObserver.onCompleted();
    }

    private static boolean hasMissingPublishFields(PublishCommentRequest request) {
        return request.getUserId().isBlank()
                || request.getReviewId().isBlank()
                || request.getContent().isBlank();
    }

    private static CommentResponse toResponse(Comment comment) {
        return CommentResponse.newBuilder()
                .setCommentId(comment.getId() == null ? "" : comment.getId())
                .setUserId(comment.getUserId())
                .setReviewId(comment.getReviewId())
                .setContent(comment.getContent())
                .setPublishedAt(comment.getPublishedAt() == null ? 0 : comment.getPublishedAt().toEpochMilli())
                .build();
    }
}
