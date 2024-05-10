using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using WestReportSystemApiReborn;

namespace WestReportToTelegram
{
    public class WestReportToTelegram : BasePlugin
    {
        public override string ModuleName => "WestReportToTelegram";
        public override string ModuleVersion => "1.0";
        public override string ModuleAuthor => "E!N";

        private IWestReportSystemApi? WRS_API;
        private TelegramConfig? _config;
        private TelegramBotClient? botClient;
        private long chatId;
        private string[]? admins;

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            string configDirectory = GetConfigDirectory();
            EnsureConfigDirectory(configDirectory);
            string configPath = Path.Combine(configDirectory, "TelegramConfig.json");
            _config = TelegramConfig.Load(configPath);

            WRS_API = IWestReportSystemApi.Capability.Get();

            if (WRS_API == null)
            {
                Console.WriteLine($"{ModuleName} | Error: Essential services (WestReportSystem API) are not available.");
                return;
            }

            InitializeTelegram();
        }

        private static string GetConfigDirectory()
        {
            return Path.Combine(Server.GameDirectory, "csgo/addons/counterstrikesharp/configs/plugins/WestReportSystem/Modules");
        }

        private void EnsureConfigDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"{ModuleName} | Created configuration directory at: {directoryPath}");
            }
        }

        private void InitializeTelegram()
        {
            if (_config == null)
            {
                Console.WriteLine($"{ModuleName} | Error: Configuration is not loaded.");
                return;
            }

            string? token = _config.TelegramToken;
            chatId = _config.TelegramChatID;
            admins = _config.TelegramAdmins;

            if (string.IsNullOrEmpty(token) || chatId == 0)
            {
                Console.WriteLine($"{ModuleName} | Error: Telegram configuration is missing (Token or Chat ID).");
                return;
            }

            botClient = new TelegramBotClient(token);
            WRS_API?.RegisterReportingModule(WRS_SendReport_To_Telegram);
            Console.WriteLine($"{ModuleName} | Initialized successfully.");
        }

        public async void WRS_SendReport_To_Telegram(CCSPlayerController sender, CCSPlayerController violator, string reason)
        {
            Console.WriteLine($"{ModuleName} | Attempting to send report...");
            string serverName = ConVar.Find("hostname")?.StringValue ?? "Unknown Server";
            string mapName = NativeAPI.GetMapName();
            string serverIp = ConVar.Find("ip")?.StringValue ?? "Unknown IP";
            string serverPort = ConVar.Find("hostport")?.GetPrimitiveValue<int>().ToString() ?? "Unknown Port";
            string siteLink = WRS_API?.GetConfigValue<string>("SiteLink") ?? "No link provided";
            int reportCount = WRS_API?.WRS_GetReportCounterPerRound(violator)?.GetValueOrDefault(violator, 1) ?? 1;

            if (botClient == null || admins == null || WRS_API == null)
            {
                Console.WriteLine($"{ModuleName} | Telegram client is not initialized.");
                return;
            }

            bool sender_prime = WRS_API.HasPrimeStatus(sender.SteamID);
            bool violator_prime = WRS_API.HasPrimeStatus(sender.SteamID);

            string messageText = $"{WRS_API.GetTranslatedText("wrtv.Title", serverName)}" +
                                 $"{WRS_API.GetTranslatedText("wrtv.Server", serverName)}" +
                                 $"{WRS_API.GetTranslatedText("wrtv.Sender", sender.PlayerName, sender.SteamID, sender_prime ? WRS_API.GetTranslatedText("wrs.PrimeTrue") : WRS_API.GetTranslatedText("wrs.PrimeFalse") ?? "Unknown")}" +
                                 $"{WRS_API.GetTranslatedText("wrtv.Violator", violator.PlayerName, violator.SteamID, violator_prime ? WRS_API.GetTranslatedText("wrs.PrimeTrue") : WRS_API.GetTranslatedText("wrs.PrimeFalse") ?? "Unknown")}" +
                                 $"{WRS_API.GetTranslatedText("wrtv.Reason", reason)}" +
                                 $"{WRS_API.GetTranslatedText("wrtv.Map", mapName, reportCount)}" +
                                 $"{WRS_API.GetTranslatedText("wrtv.Administrators", admins)}" +
                                 $"{WRS_API.GetTranslatedText("wrtv.Connect", serverIp, serverPort, siteLink)}";

            messageText = EscapeMarkdownV2(messageText);

            try
            {
                Console.WriteLine($"{ModuleName} | Sending message...");
                await botClient.SendTextMessageAsync(chatId, messageText, parseMode: ParseMode.MarkdownV2);
                Console.WriteLine($"{ModuleName} | Message sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending report to Telegram: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private string EscapeMarkdownV2(string text)
        {
            char[] specialCharacters = { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

            foreach (var character in specialCharacters)
            {
                text = text.Replace(character.ToString(), "\\" + character);
            }

            return text;
        }

        public class TelegramConfig
        {
            public string? TelegramToken { get; set; }
            public string[]? TelegramAdmins { get; set; }
            public long TelegramChatID { get; set; }

            public static TelegramConfig Load(string configPath)
            {
                if (!File.Exists(configPath))
                {
                    TelegramConfig defaultConfig = new();
                    File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented));
                    return defaultConfig;
                }

                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<TelegramConfig>(json) ?? new TelegramConfig();
            }
        }
    }
}