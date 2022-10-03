# Welcome to Jobsity Chat!

Jobsity Chat is a chat Web App with a Stock Values bot.
![Chatroom screenshot](https://i.postimg.cc/J4f8B9vx/main.png)

# App
## Fast building

### Prerequisites

 - Install [Visual Studio](https://visualstudio.microsoft.com/) or [.NET CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/)
 
### Run from Visual Studio

- Open the solution file from `src\App\JobsityChatAngular.sln`
- Press F5 key and the app will open a browser.

### Run from .NET CLI
- In a terminal, navigate to the .csproj folder using: 
 `cd .\App\JobsityChatAngular`
- Run `dotnet run`.
- Open in a browser: `https://localhost:7234`.

# JobsityChatBot 

## Prerequisites
- Previously registration on the Jobsity Chat.
- Two [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview) for queue messaging.
- A [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) for user login.

## Usage
- In a browser, navigate to `https://localhost:44477/chatroom`.
- Insert a group name, if not exists the app will create a new room.
- In the chatroom you can send bot commands like "**/stock=stock_code**", and in a few seconds the bot sends your asked stock value.

## External testing

You can test the bot without the Jobsity Chat App, using [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/blob/master/README.md), using the same commands.
Follow this [steps](https://github.com/gastonmunoz/jobsitychatapp/blob/main/utils/JobsityChatBot/README.md)

