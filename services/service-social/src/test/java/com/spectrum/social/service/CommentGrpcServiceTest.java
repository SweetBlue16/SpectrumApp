package com.spectrum.social.service;

import com.spectrum.social.grpc.CommentCountsResponse;
import com.spectrum.social.grpc.GetCommentCountsRequest;
import com.spectrum.social.repository.CommentRepository;
import io.grpc.stub.StreamObserver;
import org.bson.Document;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.aggregation.Aggregation;
import org.springframework.data.mongodb.core.aggregation.AggregationResults;

import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
class CommentGrpcServiceTest {

    @Mock
    private CommentRepository commentRepository;

    @Mock
    private MongoTemplate mongoTemplate;

    @Mock
    private StreamObserver<CommentCountsResponse> responseObserver;

    private CommentGrpcService commentGrpcService;

    @BeforeEach
    void setUp() {
        commentGrpcService = new CommentGrpcService(commentRepository, mongoTemplate);
    }

    @Test
    void getCommentCountsAggregatesByReviewIdInSingleMongoAggregation() {
        var first = new CommentGrpcService.CommentCountDocument();
        first.setReviewId("review-1");
        first.setCount(4);
        var second = new CommentGrpcService.CommentCountDocument();
        second.setReviewId("review-2");
        second.setCount(2);

        when(mongoTemplate.aggregate(
                any(Aggregation.class),
                eq("comments"),
                eq(CommentGrpcService.CommentCountDocument.class)))
                .thenReturn(new AggregationResults<>(List.of(first, second), new Document()));

        commentGrpcService.getCommentCounts(GetCommentCountsRequest.newBuilder()
                .addReviewIds("review-1")
                .addReviewIds("review-2")
                .setFrom(1000)
                .setTo(2000)
                .build(), responseObserver);

        ArgumentCaptor<CommentCountsResponse> captor = ArgumentCaptor.forClass(CommentCountsResponse.class);
        verify(responseObserver).onNext(captor.capture());
        verify(responseObserver).onCompleted();
        verify(mongoTemplate).aggregate(any(Aggregation.class), eq("comments"), eq(CommentGrpcService.CommentCountDocument.class));

        CommentCountsResponse response = captor.getValue();
        assertEquals(2, response.getCountsCount());
        assertEquals("review-1", response.getCounts(0).getReviewId());
        assertEquals(4, response.getCounts(0).getCount());
        assertEquals("review-2", response.getCounts(1).getReviewId());
        assertEquals(2, response.getCounts(1).getCount());
    }

    @Test
    void getCommentCountsWithoutReviewIdsReturnsEmptyResponseWithoutMongoQuery() {
        commentGrpcService.getCommentCounts(GetCommentCountsRequest.newBuilder().build(), responseObserver);

        ArgumentCaptor<CommentCountsResponse> captor = ArgumentCaptor.forClass(CommentCountsResponse.class);
        verify(responseObserver).onNext(captor.capture());
        verify(responseObserver).onCompleted();
        verify(mongoTemplate, never()).aggregate(any(Aggregation.class), eq("comments"), eq(CommentGrpcService.CommentCountDocument.class));

        assertEquals(0, captor.getValue().getCountsCount());
    }
}
