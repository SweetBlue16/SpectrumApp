package com.spectrum.social.model;

import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.AllArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.index.CompoundIndex;
import org.springframework.data.mongodb.core.index.Indexed;
import org.springframework.data.mongodb.core.mapping.Document;

import java.time.Instant;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Document(collection = "comments")
@CompoundIndex(name = "idx_comments_review_published", def = "{'reviewId': 1, 'publishedAt': 1}")
public class Comment {
    @Id
    private String id;
    private String userId;
    @Indexed
    private String reviewId;
    private String gameId;
    private String content;
    @Indexed
    private Instant publishedAt;
}
