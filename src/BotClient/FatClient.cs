using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FatClient
{
    public interface ITelegramBotClient
    {
        public long? BotId { get; }
    }

    public class TelegramBotClient : ITelegramBotClient
    {
        public long? BotId { get; }

        private const string BaseTelegramUrl = "https://api.telegram.org";
        public string _baseFileUrl { get; }
        public string _baseRequestUrl { get; }
        public bool _localBotServer { get; }

        private static readonly Regex TokenFormat = new Regex("^(?<token>[-]?[0-9]{1,16}):.*$", RegexOptions.Compiled);

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

            _localBotServer = TrySetBaseUrl(
                baseUrl ?? BaseTelegramUrl,
                out var effectiveBaseUrl
            );

            _baseRequestUrl = $"{effectiveBaseUrl}/bot{token}";
            _baseFileUrl = $"{effectiveBaseUrl}/file/bot{token}";
        }

        private static bool TrySetBaseUrl(string baseUrl, out string target)
        {
            if (baseUrl is null) throw new ArgumentNullException(nameof(baseUrl));

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) ||
                string.IsNullOrEmpty(baseUri.Scheme) ||
                string.IsNullOrEmpty(baseUri.Authority))
            {
                throw new ArgumentException(
                    "Invalid format. A valid base url looks \"http://localhost:8081\" ",
                    nameof(baseUrl)
                );
            }

            if (!baseUri.Host.Equals("api.telegram.org", StringComparison.Ordinal))
            {
                target = $"{baseUri.Scheme}://{baseUri.Authority}";
                return true;
            }

            target = baseUrl;
            return false;
        }
    }

    public class TelegramBotClientSpan : ITelegramBotClient
    {
        public long? BotId { get; }

        private readonly static string BaseTelegramUrl = "https://api.telegram.org";
        public string _baseFileUrl { get; }
        public string _baseRequestUrl { get; }
        public bool _localBotServer { get; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public TelegramBotClientSpan(string token, string? baseUrl = default)
        {
            if (token is null) throw new ArgumentNullException(nameof(token));

            BotId = IdFromToken(token);

            _localBotServer = baseUrl is not null;
            var effectiveBaseUrl = _localBotServer
                ? TrySetBaseUrl(baseUrl)
                : BaseTelegramUrl;

            _baseRequestUrl = $"{effectiveBaseUrl}/bot{token}";
            _baseFileUrl = $"{effectiveBaseUrl}/file/bot{token}";

            static long? IdFromToken(string token)
            {
                var span = token.AsSpan();
                var index = span.IndexOf(':');

                if (index < 1 || index > 16) return null;

                var botIdSpan = span.Slice(0, index);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                if (!long.TryParse(botIdSpan, out var botId)) return null;
#else
                if (!long.TryParse(botIdSpan.ToString(), out var botId)) return null;
#endif

                return botId;
            }
        }

        private static string TrySetBaseUrl(string? baseUrl)
        {
            if (baseUrl is null) throw new ArgumentNullException(nameof(baseUrl));

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
                || string.IsNullOrEmpty(baseUri.Scheme)
                || string.IsNullOrEmpty(baseUri.Authority))
            {
                throw new ArgumentException(
                    "Invalid format. A valid base url looks \"http://localhost:8081\" ",
                    nameof(baseUrl)
                );
            }

            return $"{baseUri.Scheme}://{baseUri.Authority}";
        }

        #region For testing purposes

        public string BaseRequestUrl => _baseRequestUrl;
        public string BaseFileUrl => _baseFileUrl;
        public bool LocalBotServer => _localBotServer;

        #endregion
    }
}
