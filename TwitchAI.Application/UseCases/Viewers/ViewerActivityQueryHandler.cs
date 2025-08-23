using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Common.Packages.Response.Models;
using Common.Packages.Logger.Services.Interfaces;
using TwitchAI.Application.Dto.Response.Activity;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Viewers
{
    public sealed class ViewerActivityQueryHandler : IRequestHandler<ViewerActivityQuery, LSResponse<DashboardActivityResponseDto>>
    {
        private readonly IExternalLogger<ViewerActivityQueryHandler> _logger;
        private readonly IViewerActivityService _activity;

        public ViewerActivityQueryHandler(IExternalLogger<ViewerActivityQueryHandler> logger, IViewerActivityService activity)
        {
            _logger = logger;
            _activity = activity;
        }

        public async Task<LSResponse<DashboardActivityResponseDto>> Handle(ViewerActivityQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(new { Method = nameof(Handle), request.UserId });

            var response = new LSResponse<DashboardActivityResponseDto>();
            try
            {
                if (request.UserId == Guid.Empty)
                {
                    return response.Error(BaseErrorCodes.IncorrectRequest, "userId is required");
                }

                var data = await _activity.GetActivityAsync(request.UserId, cancellationToken);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { Method = nameof(Handle), request.UserId, Error = ex.GetType().Name, Message = ex.Message });
                return response.Error(BaseErrorCodes.OperationProcessError, "Failed to load activity");
            }
        }
    }
}


