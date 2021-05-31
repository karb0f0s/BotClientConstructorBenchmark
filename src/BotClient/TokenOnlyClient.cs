using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace TokenOnlyClient
{
    public interface ITelegramBotClient
    {
        public long? BotId { get; }
    }

    public class TelegramBotClient : ITelegramBotClient
    {
        private static readonly Regex TokenFormat = new Regex(@"^(?<token>[-]?[0-9]{1,16}):.*$", RegexOptions.Compiled);
        public long? BotId { get; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public TelegramBotClient(string token, string? baseUrl = default)
        {
            if (token is null) throw new ArgumentNullException(nameof(token));

            var match = TokenFormat.Match(token);
            if (match.Success)
            {
                var botIdString = match.Groups["token"].Value;
                if (long.TryParse(botIdString, out var botId))
                {
                    BotId = botId;
                }
            }
        }
    }

    public class TelegramBotClientSpanLocalFunc : ITelegramBotClient
    {
        public long? BotId { get; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public TelegramBotClientSpanLocalFunc(string token, string? baseUrl = default)
        {
            if (token is null) throw new ArgumentNullException(nameof(token));

            BotId = IdFromToken(token);

            static long? IdFromToken(string token)
            {
                var span = token.AsSpan();
                var index = span.IndexOf(':');

                if (index < 1 || index > 16) return null;

                var botIdSpan = span.Slice(0, index);
#if NETCOREAPP3_1_OR_GREATER
            if (!long.TryParse(botIdSpan, out var botId)) return null;
#else
                if (!long.TryParse(botIdSpan.ToString(), out var botId)) return null;
#endif

                return botId;
            }
        }
    }
}
