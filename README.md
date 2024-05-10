# WestReportToTelegram
Modular report system for your CS:2 server - WestReportSystem

**Sending reports to Telegram**

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
