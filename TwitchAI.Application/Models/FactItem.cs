namespace TwitchAI.Application.Models
{
    /// <summary>
    /// Модель факта с отслеживанием использования
    /// </summary>
    public class FactItem
    {
        /// <summary>
        /// Текст факта
        /// </summary>
        public string Text { get; set; } = string.Empty;
        
        /// <summary>
        /// Флаг того, что факт уже был использован
        /// </summary>
        public bool IsUsed { get; set; } = false;
        
        /// <summary>
        /// Время последнего использования факта
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// Конструктор для создания факта
        /// </summary>
        public FactItem(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Отметить факт как использованный
        /// </summary>
        public void MarkAsUsed()
        {
            IsUsed = true;
            LastUsedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Сбросить флаг использования
        /// </summary>
        public void ResetUsage()
        {
            IsUsed = false;
            LastUsedAt = null;
        }
    }
} 