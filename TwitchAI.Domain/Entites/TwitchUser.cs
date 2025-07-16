using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites;

/// <summary>
/// Twitch user
/// </summary>
public class TwitchUser : Entity
{
    /* ---------- основные идентификаторы ---------------- */
    /// <summary>Цифровой ID в Twitch (tag <c>user-id</c>).</summary>
    public string TwitchId { get; set; } = null!;

    /// <summary>Логин (lower-case).</summary>
    public string UserName { get; set; } = null!;

    /// <summary>Отображаемое имя (может содержать регистр/юникод).</summary>
    public string DisplayName { get; set; } = null!;

    /* ---------- внешнее оформление -------------------- */
    /// <summary>HEX-цвет ника, если задан пользователем.</summary>
    public string ColorHex { get; set; } = null!;   // «#FF4500»

    /* ---------- постоянные роли / бейджи -------------- */
    public bool? IsVip { get; set; }
    public bool? IsPartner { get; set; }
    public bool? IsStaff { get; set; }
    public bool? IsBroadcaster { get; set; }
    public bool? IsTurbo { get; set; }

    public int? SubscribedMonthCount { get; set; }

    public int? CheerBits{ get; set; }

    /// <summary>Последний актуальный набор бейджей пользователя (JSON).</summary>
    public string BadgesJson { get; set; } = null!;   // [{"vip":"1"}, {"partner":"1"}]

    /* ---------- технические поля ----------------------- */
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    /* ---------- навигация ------------------------------ */
    public virtual ICollection<ChatMessage> ChatMessages { get; set; }
        = new List<ChatMessage>();

    /// <summary>
    /// История диалогов пользователя с GPT
    /// </summary>
    public virtual ICollection<ConversationMessage> ConversationMessages { get; set; }
        = new List<ConversationMessage>();
}