# WestReportToTelegram
Modular report system for your CS:2 server - WestReportSystem

**Sending reports to Telegram**

![image](https://github.com/Stimayk/WestReportToTelegram/assets/51941742/6a8cb407-f5e8-4d35-beb9-3e67142f7264)


Uses [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot)

The WRTTG module itself is also customized
Configuration file:
```
{
  "TelegramToken": "", // Bot token, via https://t.me/botfather
  "TelegramAdmins": ["admin", "admin2"],
  "TelegramChatID": -1002060923245 // https://t.me/LeadConverterToolkitBot - To get a Chat ID (mandatory with -)
}
```

Installing the module:
+ Download the archive from the [releases](https://github.com/Stimayk/WestReportToTelegram/releases)
+ Unzip to plugins
+ Customize the module in /configs/plugins
+ Customize translations if necessary
