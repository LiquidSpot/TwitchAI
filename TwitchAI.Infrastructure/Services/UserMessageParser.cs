using System.Text.RegularExpressions;

using TwitchAI.Application.Interfaces;

using TwitchAI.Domain.Entites;

using TwitchAI.Domain.Enums;

namespace TwitchAI.Infrastructure.Services
{
    internal sealed class UserMessageParser : IUserMessageParser
    {
        private const string FullCmd = @"^([!\w.]+)\s+([1-9][0-9]?|100)\s+(.+)$";
        private const string OneCmd = @"^([!\w.]+)\s+(.+)$";

        private static readonly Regex _full = new(FullCmd, RegexOptions.Compiled);
        private static readonly Regex _one = new(OneCmd, RegexOptions.Compiled);

        private Role _currentRole = Role.Bot;
        public Role CurrentRole => _currentRole;

        public bool TryParse(string raw, out UserMessage usrMsg)
        {
            usrMsg = new UserMessage { role = _currentRole };

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var parts = raw.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);

            // --- смена дефолт-роли ---
            if (parts.Length == 2 && Enum.TryParse(parts[1], true, out Role newRole))
            {
                _currentRole = newRole;
                usrMsg.role = _currentRole;
                usrMsg.message = $"Роль по умолчанию изменена на {_currentRole} 🐾";
                return true;
            }

            // --- !ai Bot Привет ---
            if (parts.Length >= 3 && parts[0].Equals("!ai", StringComparison.OrdinalIgnoreCase)
                                  && Enum.TryParse(parts[1], true, out Role explicitRole))
            {
                usrMsg.role = explicitRole;
                usrMsg.message = parts[2];
                return true;
            }

            // --- расширенные шаблоны ---
            if (_full.Match(raw) is { Success: true } mf)
            {
                FillFromMatch(mf, ref usrMsg, full: true);
                return true;
            }

            if (_one.Match(raw) is { Success: true } mo)
            {
                FillFromMatch(mo, ref usrMsg, full: false);
                return true;
            }

            // --- обычный текст ---
            usrMsg.message = raw;
            return true;
        }

        private static void FillFromMatch(Match m, ref UserMessage u, bool full)
        {
            var rStr = m.Groups[1].Value;
            var arg2 = full ? m.Groups[2].Value : null;
            var text = full ? m.Groups[3].Value : m.Groups[2].Value;

            if (Enum.TryParse(rStr, true, out Role role))
                u.role = role;
            else if (double.TryParse(rStr, out var t1))
                u.temp = Normalize(t1);

            if (full && double.TryParse(arg2, out var t2))
                u.temp = Normalize(t2);

            u.message = text;
        }

        private static double Normalize(double percent) =>
            Math.Clamp(percent / 100.0 * 2.0, 0.1, 2.0);
    }
}
