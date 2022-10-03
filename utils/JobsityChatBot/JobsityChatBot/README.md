# JobsityChatBot

JobsityChatBot is a chatroom bot for ask Stocks

### Overview

### Install .NET CLI

- [.NET SDK](https://dotnet.microsoft.com/download) version 6.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```

- If you don't have a Bot Framework Emulator, you can [Install Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/blob/master/README.md).

## To try this sample

- In a terminal, navigate to `JobsityChatBot`

    ```bash
    # change into project folder
    cd JobsityChatBot
    ```

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `JobsityChatBot` folder
  - Select `JobsityChatBot.csproj` file
  - Press `F5` to run the project

- Copy the port number in the line before Application started. Press CTRL+C to shut down.

  ```bash
  Now listening on: http://localhost:3978
  Application started. Press Ctrl+C to shut down.
  ```

- Open the Bot Framework Emulator, press Open Bot and paste the bot URL endpoint.
  
  ```bash
  http://localhost:3978/api/messages
  ```

- Type /stock=AAPL.US and the bot will ask stooq.com for the current values of Apple stock
