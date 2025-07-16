using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;

namespace TwitchAI.Application.Interfaces;

internal interface IChatCommand : ICommand<LSResponse<string>>
{ }