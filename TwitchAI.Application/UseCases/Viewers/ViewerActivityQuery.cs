using System;
using MediatR;
using Common.Packages.Response.Models;
using TwitchAI.Application.Dto.Response.Activity;

namespace TwitchAI.Application.UseCases.Viewers
{
    public sealed record ViewerActivityQuery(Guid UserId) : IRequest<LSResponse<DashboardActivityResponseDto>>;
}


