package com.spectrum.social.repository;

import com.spectrum.social.model.Report;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface ReportRepository extends MongoRepository<Report, String> {
    List<Report> findByStatus(String status);
    boolean existsByReporterIdAndTargetId(String reporterId, String targetId);
}
