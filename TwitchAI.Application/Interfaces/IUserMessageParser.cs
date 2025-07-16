using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums;

namespace TwitchAI.Application.Interfaces
{
    public interface IUserMessageParser
    {
        /// <summary>Текущая «дефолт-роль» (меняется командой вида <c>!ai Neko</c>).</summary>
        Role CurrentRole { get; }

        /// <summary>Пытается разобрать входной текст в <see cref="UserMessage"/>.</summary>
        bool TryParse(string raw, out UserMessage message);
    }
}
