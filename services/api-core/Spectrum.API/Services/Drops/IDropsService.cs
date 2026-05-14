using Grpc.Core;
using Spectrum.API.Dtos.Drops;
using Spectrum.API.Exceptions;
using Spectrum.API.Grpc.Drops;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.Drops
{
    public interface IDropsService
    {
        Task CreateEventAsync(CreateDropEventDto dto, CancellationToken cancellationToken);
        Task UpdateEventAsync(string eventId, UpdateDropEventDto dto, CancellationToken cancellationToken);
        Task<WonKeyDto?> ClaimAccessKeyAsync(Guid userId, string eventId, CancellationToken cancellationToken);
        Task<EventStatusDto> GetEventStatusAsync(string eventId, CancellationToken cancellationToken);
        Task<IEnumerable<WonKeyDto>> GetUserWonKeysAsync(Guid userId, CancellationToken cancellationToken);
    }

    public class DropsService : IDropsService
    {
        private readonly DropService.DropServiceClient _dropServiceClient;
        private readonly ILogger<DropsService> _logger;

        public DropsService(DropService.DropServiceClient dropServiceClient, ILogger<DropsService> logger)
        {
            _dropServiceClient = dropServiceClient;
            _logger = logger;
        }

        public async Task<WonKeyDto?> ClaimAccessKeyAsync(Guid userId, string eventId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _dropServiceClient.ClaimAccessKeyAsync(new ClaimKeyRequest
                {
                    UserId = userId.ToString(),
                    EventId = eventId
                }, cancellationToken: cancellationToken);

                if (!response.Success) return null;

                return new WonKeyDto
                {
                    EventId = eventId,
                    AccessKeyCode = response.AccessKeyCode,
                    ClaimedAt = DateTimeOffset.FromUnixTimeMilliseconds(response.ClaimedAt).UtcDateTime
                };
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error calling gRPC ClaimAccessKey");
                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable);
            }
        }

        public async Task CreateEventAsync(CreateDropEventDto dto, CancellationToken cancellationToken)
        {
            var request = new CreateEventRequest
            {
                GameTitle = dto.GameTitle,
                CoverImageUrl = dto.CoverImageUrl,
                EndDate = new DateTimeOffset(dto.EndDate).ToUnixTimeMilliseconds()
            };
            request.AccessKeys.AddRange(dto.AccessKeys);

            var response = await _dropServiceClient.CreateEventAsync(request, cancellationToken: cancellationToken);
            if (!response.Success) throw new SpectrumBusinessException(response.Message);
        }

        public async Task<EventStatusDto> GetEventStatusAsync(string eventId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _dropServiceClient.GetEventStatusAsync(new GetEventRequest 
                {
                    EventId = eventId
                }, cancellationToken: cancellationToken);

                return new EventStatusDto
                {
                    EventId = response.EventId,
                    KeysAvailable = response.KeysAvailable,
                    KeysTotal = response.KeysTotal,
                    Status = response.Status,
                    EndDate = DateTimeOffset.FromUnixTimeMilliseconds(response.EndDate).UtcDateTime
                };
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error calling gRPC GetEventStatus");
                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable);
            }
        }

        public async Task<IEnumerable<WonKeyDto>> GetUserWonKeysAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _dropServiceClient.GetWonKeysAsync(new WonKeysRequest 
                { 
                    UserId = userId.ToString() 
                }, cancellationToken: cancellationToken);

                return response.WonKeys.Select(k => new WonKeyDto
                {
                    EventId = k.EventId,
                    GameTitle = k.GameTitle,
                    AccessKeyCode = k.AccessKeyCode,
                    ClaimedAt = DateTimeOffset.FromUnixTimeMilliseconds(k.ClaimedAt).UtcDateTime
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error calling gRPC GetWonKeys");
                return Enumerable.Empty<WonKeyDto>();
            }
        }

        public async Task UpdateEventAsync(string eventId, UpdateDropEventDto dto, CancellationToken cancellationToken)
        {
            var request = new UpdateEventRequest
            {
                EventId = eventId,
                GameTitle = dto.GameTitle,
                CoverImageUrl = dto.CoverImageUrl,
                EndDate = new DateTimeOffset(dto.EndDate).ToUnixTimeMilliseconds(),
                Status = dto.Status
            };

            var response = await _dropServiceClient.UpdateEventAsync(request, cancellationToken: cancellationToken);
            if (!response.Success) throw new SpectrumBusinessException(response.Message);
        }
    }
}
