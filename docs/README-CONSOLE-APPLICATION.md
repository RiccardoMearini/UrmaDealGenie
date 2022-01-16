# Run as a .NET Core Console Application
This application requires .NET 6.0 Runtime:
  - [Windows x64 .NET 6.0 installer](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-6.0.0-windows-x64-installer)
  - [MacOS x64 .NET 6.0 installer](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-6.0.0-macos-x64-installer)
  - For other installers, go to [.NET 6.0 runtime](https://dotnet.microsoft.com/download/dotnet/6.0) and download appropriate **installer** under ".NET Runtime 6.0.0
" section
## Setup and Run
_First off, **SORRY** this is so complicated!! I'm working on making this one-click!_

To install and configure the Urma Deal Genie console application:
1. Download [UrmaDealGenieApp release files](https://github.com/UrmaGurd/UrmaDealGenie/releases/tag/app-2.3 ) depending on your operating system file to a local folder, e.g. `c:\UrmaDealGenieApp`
   - `appsettings.json`
   - `dealrules.json`
   - `UrmaDealGenieApp-win10-x64.exe`, or `UrmaDealGenieApp-osx-x64` or `UrmaDealGenieApp-linux-x64`
4. Edit `dealrules.json` file on your machine to match your bots. See [docs/ExampleConfigs.md](./ExampleConfigs.md) for example configs with detailed explanations of rule settings
5. Open a command/terminal window:
   - On Windows go to Start->Command Prompt
   - On Mac launch Terminal
   - On Linux... I dunno, google it?
   - navigate to the folder you downloaded the files to (e.g. `cd c:\UrmaDealGenieApp`)
6. In the console/terminal window, set environment variables for the 3Commas [APIKEY and SECRET](/README.md#create-a-3commas-api-key-and-secret), and optionally [CMCAPIKEY](/README.md#optionally-create-a-coinmarketcap-api-key) for the [LunarCrush rule](ExampleConfigs-LunarCrushPairRule.md)
   - **Windows**
     ```
     set APIKEY=YOUR_API_KEY_HERE
     set SECRET=YOUR_3COMMAS_SECRET_HERE
     set CMCAPIKEY=OPTIONAL_CMC_API_KEY_HERE
     ```
   - **Mac/Linux**
     ```
     export APIKEY=YOUR_API_KEY_HERE
     export SECRET=YOUR_3COMMAS_SECRET_HERE
     export CMCAPIKEY=OPTIONAL_CMC_API_KEY_HERE
     ```
5. Run UrmaDealGenieApp depending on your operating system, Windows, Mac or Linux:
     ```
     UrmaDealGenieApp-win10-x64.exe
     UrmaDealGenieApp-osx-x64
     UrmaDealGenieApp-linux-x64
     ```
That's it. It will run in until you stop it by pressing Ctrl+C

You can put these 3 commands in a .bat file or .sh script to make it easier. Here is the Windows .bat file example below (e.g. `run-genie.bat`), the Mac and Linux .sh example you can work out I'm sure:
 ```
 set APIKEY=YOUR_API_KEY_HERE
 set SECRET=YOUR_3COMMAS_SECRET_HERE
 set CMCAPIKEY=OPTIONAL_CMC_API_KEY_HERE
 UrmaDealGenieApp-win10-x64.exe
 ```
