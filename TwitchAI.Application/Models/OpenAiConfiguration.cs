namespace TwitchAI.Application.Models;

public class OpenAiConfiguration
{
    public string OrganizationId { get; set; } = "your_openai_organization_id";
    public string ProjectId { get; set; } = "your_openai_project_id";
    
    /// <summary>
    /// Модель по умолчанию для запросов к OpenAI
    /// </summary>
    public string Model { get; set; } = "gpt-4.1-2025-04-14";
    
    /// <summary>
    /// Максимальное количество токенов по умолчанию
    /// </summary>
    public int MaxTokens { get; set; } = 512;
    
    /// <summary>
    /// Температура по умолчанию для генерации текста
    /// </summary>
    public double Temperature { get; set; } = 0.8;
    
    /// <summary>
    /// Список доступных движков OpenAI
    /// </summary>
    public string[] AvailableEngines { get; set; } = { "gpt-4o-2024-11-20", "gpt-4.1-2025-04-14", "chatgpt-4o-latest", "o4-mini-2025-04-16", "o3-2025-04-16" };
} 