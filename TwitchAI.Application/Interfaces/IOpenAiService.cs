using Common.Packages.Response.Models;

using TwitchAI.Application.Dto.Response;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums;

namespace TwitchAI.Application.Interfaces;

public interface IOpenAiService
{


    /// <summary>
    /// Универсальный метод генерации с контекстом пользователя
    /// </summary>
    /// <param name="message">Сообщение пользователя</param>
    /// <param name="conversationContext">Контекст диалога пользователя</param>
    /// <param name="apiVersion">Версия API для использования (если не указана, берется из конфигурации)</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Текстовый ответ от OpenAI</returns>
    public Task<LSResponse<string>> GenerateUniversalWithContextAsync(UserMessage message, List<ConversationMessage> conversationContext, OpenAiApiVersion? apiVersion = null, CancellationToken ct = default);


}