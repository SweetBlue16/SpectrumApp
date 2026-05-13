using Grpc.Core;
using Spectrum.API.Dtos.Reports;
using Spectrum.API.Exceptions;
using Spectrum.API.Grpc.Social;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.Reports
{
    public interface IReportService
    {
        Task SubmitReportAsync(Guid reporterId, CreateReportDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<ReportDetailsDto>> GetReportsByStatusAsync(string status, CancellationToken cancellationToken = default);
        Task UpdateReportStatusAsync(string reportId, Guid moderatorId, UpdateReportStatusDto dto, CancellationToken cancellationToken = default);
    }

    public class ReportsService : IReportService
    {
        private readonly ReportService.ReportServiceClient  _reportServiceClient;
        private readonly ILogger<ReportsService> _logger;

        public ReportsService(ReportService.ReportServiceClient reportServiceClient, ILogger<ReportsService> logger)
        {
            _reportServiceClient = reportServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<ReportDetailsDto>> GetReportsByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            var reports = new List<ReportDetailsDto>();
            try
            {
                var request = new ListReportsRequest { Status = status };
                using var call = _reportServiceClient.ListReportsByStatus(request, cancellationToken: cancellationToken);

                await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    reports.Add(new ReportDetailsDto
                    {
                        ReportId = response.ReportId,
                        ReporterId = Guid.Parse(response.ReporterId),
                        TargetId = Guid.Parse(response.TargetId),
                        TargetType = response.TargetType,
                        Reason = response.Reason,
                        Status = response.Status,
                        ReportedAt = DateTimeOffset.FromUnixTimeMilliseconds(response.ReportedAt).UtcDateTime
                    });
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC streaming failed for reports.");
                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable);
            }
            return reports;
        }

        public async Task SubmitReportAsync(Guid reporterId, CreateReportDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new SubmitReportRequest
                {
                    ReporterId = reporterId.ToString(),
                    TargetId = dto.TargetId.ToString(),
                    TargetType = dto.TargetType,
                    Reason = dto.Reason
                };

                var response = await _reportServiceClient.SubmitReportAsync(request, cancellationToken: cancellationToken);
                if (!response.Success)
                {
                    throw new SpectrumBusinessException(response.Message);
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to connect to Social gRPC service.");
                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable);
            }
        }

        public async Task UpdateReportStatusAsync(string reportId, Guid moderatorId, UpdateReportStatusDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new UpdateReportStatusRequest
                {
                    ReportId = reportId,
                    ModeratorId = moderatorId.ToString(),
                    NewStatus = dto.NewStatus,
                    ResolutionNotes = dto.ResolutionNotes ?? string.Empty
                };

                var response = await _reportServiceClient.UpdateReportStatusAsync(request, cancellationToken: cancellationToken);

                if (!response.Success)
                {
                    if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                        throw new SpectrumNotFoundException(response.Message);

                    throw new SpectrumBusinessException(response.Message);
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to connect to Social gRPC service.");
                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable);
            }
        }
    }
}