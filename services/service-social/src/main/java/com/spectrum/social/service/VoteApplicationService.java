package com.spectrum.social.service;

import com.spectrum.social.model.Vote;
import com.spectrum.social.repository.VoteRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
public class VoteApplicationService {

    private final VoteRepository voteRepository;

    public VoteCounts castVote(String userId, String targetId, String targetType, boolean positive) {
        voteRepository.findByUserIdAndTargetIdAndTargetType(userId, targetId, targetType)
                .ifPresentOrElse(
                        existingVote -> applyExistingVote(existingVote, positive),
                        () -> createVote(userId, targetId, targetType, positive)
                );

        return getVoteCounts(targetId, targetType);
    }

    private void applyExistingVote(Vote existingVote, boolean positive) {
        if (existingVote.isPositive() == positive) {
            voteRepository.delete(existingVote);
            return;
        }

        existingVote.setPositive(positive);
        voteRepository.save(existingVote);
    }

    private void createVote(String userId, String targetId, String targetType, boolean positive) {
        Vote vote = Vote.builder()
                .userId(userId)
                .targetId(targetId)
                .targetType(targetType)
                .positive(positive)
                .build();

        voteRepository.save(vote);
    }

    private VoteCounts getVoteCounts(String targetId, String targetType) {
        long likes = voteRepository.countByTargetIdAndTargetTypeAndPositive(targetId, targetType, true);
        long dislikes = voteRepository.countByTargetIdAndTargetTypeAndPositive(targetId, targetType, false);

        return new VoteCounts(toIntCount(likes), toIntCount(dislikes));
    }

    private static int toIntCount(long count) {
        return Math.toIntExact(count);
    }

    public record VoteCounts(int updatedLikes, int updatedDislikes) {
    }
}
