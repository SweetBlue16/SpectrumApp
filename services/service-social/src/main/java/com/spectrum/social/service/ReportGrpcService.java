package com.spectrum.social.service;

import com.spectrum.social.grpc.ReportServiceGrpc;
import com.spectrum.social.grpc.SubmitReportRequest;
import com.spectrum.social.grpc.ReportResponse;
import com.spectrum.social.grpc.ListReportsRequest;
import com.spectrum.social.grpc.ReportDetails;
import com.spectrum.social.grpc.UpdateReportStatusRequest;
import com.spectrum.social.grpc.ReportActionResponse;
import com.spectrum.social.model.Report;
import com.spectrum.social.repository.ReportRepository;
import io.grpc.stub.StreamObserver;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import net.devh.boot.grpc.server.service.GrpcService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.dao.DataAccessException;

import java.time.Instant;
import java.util.List;
import java.util.Optional;

@GrpcService
@RequiredArgsConstructor
@Slf4j
public class ReportGrpcService extends ReportServiceGrpc.ReportServiceImplBase {

    private static final Logger logger = LoggerFactory.getLogger(ReportGrpcService.class);

    private final ReportRepository reportRepository;

    @Override
    public void submitReport(SubmitReportRequest request, StreamObserver<ReportResponse> responseObserver) {
        try {
            validateSubmitReportRequest(request);

            if (reportRepository.existsByReporterIdAndTargetId(request.getReporterId(), request.getTargetId())) {
                throw new IllegalStateException("You have already reported this content.");
            }

            Report report = new Report();
            report.setReporterId(request.getReporterId());
            report.setTargetId(request.getTargetId());
            report.setTargetType(request.getTargetType());
            report.setReason(request.getReason());
            report.setStatus("PENDING");
            report.setReportedAt(Instant.now());

            reportRepository.save(report);

            responseObserver.onNext(ReportResponse.newBuilder()
                    .setSuccess(true)
                    .setMessage("Report submitted successfully.")
                    .build());
            responseObserver.onCompleted();
        } catch (IllegalArgumentException e) {
            logger.warn("Validation error on submitReport: {}", e.getMessage());
            sendErrorResponse(responseObserver, e.getMessage());
        } catch (IllegalStateException e) {
            logger.warn("Business rule violation on submitReport: {}", e.getMessage());
            sendErrorResponse(responseObserver, e.getMessage());
        } catch (DataAccessException e) {
            logger.error("Database error while saving report for user {}", request.getReporterId(), e);
            sendErrorResponse(responseObserver, "A database error occurred while saving the report. Please try again.");
        } catch (Exception e) {
            logger.error("Unexpected error saving report to MongoDB", e);
            sendErrorResponse(responseObserver, "An internal server error occurred while processing the report.");
        }
    }

    @Override
    public void listReportsByStatus(ListReportsRequest request, StreamObserver<ReportDetails> responseObserver) {
        try {
            if (request.getStatus().isBlank()) {
                throw new IllegalArgumentException("Status filter cannot be blank.");
            }

            List<Report> reports = reportRepository.findByStatus(request.getStatus());

            for (Report report : reports) {
                ReportDetails details = ReportDetails.newBuilder()
                        .setReportId(report.getId())
                        .setReporterId(report.getReporterId())
                        .setTargetId(report.getTargetId())
                        .setTargetType(report.getTargetType())
                        .setReason(report.getReason())
                        .setStatus(report.getStatus())
                        .setReportedAt(report.getReportedAt().toEpochMilli())
                        .build();
                responseObserver.onNext(details);
            }
            responseObserver.onCompleted();
        } catch (IllegalArgumentException e) {
            logger.warn("Validation error on listReportsByStatus: {}", e.getMessage());
            responseObserver.onCompleted();
        } catch (DataAccessException e) {
            logger.error("Database error fetching reports with status {}", request.getStatus(), e);
            responseObserver.onCompleted();
        } catch (Exception e) {
            logger.error("Unexpected error fetching reports stream", e);
            responseObserver.onCompleted();
        }
    }

    @Override
    public void updateReportStatus(UpdateReportStatusRequest request, StreamObserver<ReportActionResponse> responseObserver) {
        try {
            validateUpdateStatusRequest(request);

            Optional<Report> optionalReport = reportRepository.findById(request.getReportId());

            if (optionalReport.isPresent()) {
                Report report = optionalReport.get();
                report.setStatus(request.getNewStatus());
                report.setModeratorId(request.getModeratorId());
                report.setResolutionNotes(request.getResolutionNotes());

                reportRepository.save(report);

                responseObserver.onNext(ReportActionResponse.newBuilder()
                        .setSuccess(true)
                        .setMessage("Report status updated to " + request.getNewStatus())
                        .build());
            } else {
                throw new IllegalArgumentException("Report not found.");
            }
            responseObserver.onCompleted();
        } catch (IllegalArgumentException e) {
            logger.warn("Validation error on updateReportStatus: {}", e.getMessage());
            responseObserver.onNext(ReportActionResponse.newBuilder()
                    .setSuccess(false)
                    .setMessage(e.getMessage())
                    .build());
            responseObserver.onCompleted();
        } catch (DataAccessException e) {
            logger.error("Database error updating report {}", request.getReportId(), e);
            responseObserver.onNext(ReportActionResponse.newBuilder()
                    .setSuccess(false)
                    .setMessage("A database error occurred while updating the report.")
                    .build());
            responseObserver.onCompleted();
        } catch (Exception e) {
            logger.error("Unexpected error updating report status", e);
            responseObserver.onNext(ReportActionResponse.newBuilder()
                    .setSuccess(false)
                    .setMessage("An internal error occurred.")
                    .build());
            responseObserver.onCompleted();
        }
    }

    private void validateSubmitReportRequest(SubmitReportRequest request) {
        if (request.getReporterId().isBlank() || request.getTargetId().isBlank() || request.getReason().isBlank()) {
            throw new IllegalArgumentException("Missing required fields. ReporterId, TargetId, and Reason are mandatory.");
        }
        if (!request.getTargetType().equals("REVIEW") && !request.getTargetType().equals("COMMENT")) {
            throw new IllegalArgumentException("Invalid TargetType. Must be REVIEW or COMMENT.");
        }
    }

    private void validateUpdateStatusRequest(UpdateReportStatusRequest request) {
        if (request.getReportId().isBlank() || request.getNewStatus().isBlank() || request.getModeratorId().isBlank()) {
            throw new IllegalArgumentException("Missing required fields for updating report status.");
        }
        if (!request.getNewStatus().equals("RESOLVED") && !request.getNewStatus().equals("DISMISSED")) {
            throw new IllegalArgumentException("Invalid NewStatus. Must be RESOLVED or DISMISSED.");
        }
    }

    private void sendErrorResponse(StreamObserver<ReportResponse> observer, String message) {
        observer.onNext(ReportResponse.newBuilder()
                .setSuccess(false)
                .setMessage(message)
                .build());
        observer.onCompleted();
    }
}
