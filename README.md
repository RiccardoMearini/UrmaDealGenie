# Urma-Gurd's 3Commas Deal Genie
This is a simple function that runs every few minutes, and updates configured 3Commas deals to change the Take Profit % (TP) based on sets of rules.  It can be run as a cloud hosted Amazon Web Services (AWS) Lambda Function (preferred!), a standalone console application or a Docker container.

Typically Urma Deal Genie is used to automatically increase TP% as your bot deals buy more safety orders, so that they take advantage of the extra volume and volatility to make more profit. This strategy works really well with the [Urma DCA bot settings](docs/UrmaBotSettings.md) which scale volume at a high rate over a small number of safety orders. 

**NOTE:** Currently this doesn't work with Paper Trade accounts - thanks to "De smikkelman" for discovering this limitation! Eventually I'll fix this.

# Donations
If you got some value out of this project, please consider donating. A lot of time and effort went into creating this, and I'm hoping to expand the functionality to include more deal rules and functionality.
- [BuyMeACoffee/UrmaGurd](https://www.buymeacoffee.com/UrmaGurd)
- Binance Pay ID: 74725526 ([See how easy it is](https://www.binance.com/en/support/faq/b3fa3ae045b9429084203c3a4ff1362f))
- Crypto Wallets:
  - LTC: MQ7gP6gme9TYgRr6kHHqwxzMWRyUpgQ5dC
  - BUSD: 0xac48d07fa2738121ca4ad0e79a764eadf515fa58	(BEP20 network or ERC20)
  - USDT: 0xac48d07fa2738121ca4ad0e79a764eadf515fa58	(BEP20 network or ERC20)
- Contact me on Discord `Urma-Gurd#6475` if you want to donate other cryptocurrencies

# Deployment Options
You can deploy Urma Deal Genie as an AWS Lambda Function (serverless), as a .NET Core application that runs on Mac/Linux/Windows, or as a Docker container.
![Urma Deal Genie deployment diagrams](https://user-images.githubusercontent.com/13062477/147111837-617c9ed1-47a8-43ef-a338-c40e96e5d582.png)
The AWS Lambda function requires an AWS account (free). The application requires .NET 6.0 installed. The Docker container requires Docker Desktop installed. 

# Getting Started
You will need the following (full details further below):
1. A 3Commas account that is setup with at least 1 active DCA bot
1. A new 3Commas API key and secret
1. Urma Deal Genie release, either:
   - `UrmaDealGenieAWS-{ver}.zip` AWS Lambda function package, or
   - `UrmaDealGenieApp-win10-64.exe`, `UrmaDealGenieApp-osx-64` or `UrmaDealGenieApp-linux-x64` console application, or
   - `urmagurd/deal-genie:{ver}` Docker image

See [UrmaDealGenie Releases](https://github.com/UrmaGurd/UrmaDealGenie/releases) for the zips, or [urmagurd/deal-genie Docker registry](https://hub.docker.com/repository/docker/urmagurd/deal-genie) for the Docker image.

## 3Commas DCA Bots
If you haven't already, go checkout [TradeAlt's Trading Bots](https://youtu.be/ziy-9yYTrbc) tutorial YouTube playlist. It will get you setup with DCA trading bots with a safe and effective passive income trading bot setup. I cannot stress enough how important this guy's video series is! 

The Urma Deal Genie works with DCA bot deals, and uses bot names to determine which deals to apply rules to. So try to name your bots with meaningful names that represent the strategy of that bot. e.g. "TA Safer BUSD" or "BTC HODL" or "Urma 250"

Checkout the [Urma DCA bot settings](docs/UrmaBotSettings.md) for a low budget, higher profit alternative to TradeAlt's settings.

### Create a 3Commas API key and secret
The Urma Deal Genie needs to connect to your 3Commas account, and it needs 
1. Go to https://3commas.io/api_access_tokens and click "New API access token" 
1. Give it a name like "UrmaDealGenie"
1. Tick "Bots read" and "Bots write"
1. Take a note of the API Key and Secret, you'll need them later in the instructions

# Run in the Cloud using AWS Lambda Function (preferred)
It's actually easier than you think, and my CloudFormation template makes it much easier to install in AWS now. In summary this is what you do:
1. Create a [new account in  AWS (free)](https://aws.amazon.com/free/)
1. Create an “S3 bucket” to upload files to (easy)
1. Upload 3 files from my release:
    - `UrmaDealGenieAWS-{ver}.zip` release package file
    - `dealrules.json` file, modified with your rules
    - `deploy-urmadealgenie.yml` CloudFormation template file which tells AWS what how to deploy Urma Deal Genie
1. Deploy Urma Deal Genie with my CloudFormation template file (it's really easy), specifying your 3c API key & secret

And that's it. Detailed steps below, and feel free to DM me on Discord if you get stuck or just want some guidance. I'm always happy to help people set this up.

## 1. Create an AWS Account
Amazon Web Services (AWS) has a free tier that allows several services to run free of charge. One of those free services is "Lambda functions". These are serverless functions that allow code to run in the cloud in your own AWS account, without being logged in or needing a PC running 24/7. 
1. Go to [AWS Free Tier](https://aws.amazon.com/free/) and sign up
1. You will need a credit card, but this is to prevent fraud and bots (see [AWS Free Tier FAQs](https://aws.amazon.com/free/registration-faqs/) )
1. Make sure you secure your account with 2FA

## 2. Create an S3 Bucket
An S3 bucket is like cloud file storage. You create a "bucket" which is a bit like a folder, and you upload files to it. Each bucket has a globally unique name (whatever you name it, there won't be another with the same name in the world).
1. Go to [S3 Management Console](https://s3.console.aws.amazon.com) and click "Create bucket"
1. Give the bucket a name (e.g. `urmadealgenie-somerandomnumbers`)
1. Choose a region (doesn't really matter, but just remember which region you choose, e.g. `eu-west-2` London)
1. Leave everything else default (it will be private and secure) and click "Create bucket" 

## 3. Upload Urma Deal Genie files to S3 Bucket
An AWS Lambda function is code that runs in the cloud, but first you need to download Urma Deal Genie files and upload them to your S3 bucket.
1. Download the Urma Deal Genie files from my GitHub to your PC:
    - [UrmaDealGenieAWS-2.3.zip](https://github.com/UrmaGurd/UrmaDealGenie/releases/download/2.3/UrmaDealGenieAWS-2.3.zip)
    - [dealrules.json](https://github.com/UrmaGurd/UrmaDealGenie/releases/download/2.3/dealrules.json)
    - [deploy-urmadealgenie.yml](https://github.com/UrmaGurd/UrmaDealGenie/releases/download/2.3//deploy-urmadealgenie.yml)
1. Go to [S3 Management Console](https://s3.console.aws.amazon.com/s3) and click on your bucket that you created above.
1. Click "Upload" and choose the 3 files above that you downloaded to the S3 bucket (drag and drop them, or browse for them), and click "Upload"
1. Click on the `deploy-urmadealgenie.yml` file and copy the `Object URL` 


## 4. Deploy Urma Deal Genie with CloudFormation
An AWS CloudFormation stack is a bit like install kits for AWS. You create a "stack" from a template file, and AWS creates all the services and wires them up as per the template file.
1. Go to [CloudFormation](https://console.aws.amazon.com/cloudformation) and click "Create stack"
1. Enter the S3 URL to the `deploy-urmadealgenie.yml` file, or just upload the file you downloaded
1. Enter a `Stack name` (e.g. UrmaDealGenie, or make up whatever you want)
1. Enter Parameters:
    - `ApiKey` (that you created above in 3Commas)
    - `ApiSecret` (ditto)
    - `S3Bucket` (name of the bucket you created)
    - `S3PackageFilename` (name of the UrmaDealGenie zip file you uploaded to the bucket)
1. Click "Next" twice, review the stack settings
1. Tick "I acknowledge that AWS CloudFormation..." and click "Create stack"
1. It takes a few minutes, so wait until it says "CREATE_COMPLETE".

And that's it! The Urma Deal Genie will run every 5 minutes and use the `dealrules.json` file in your bucket as the rule configuration.
## Monitoring Urma Deal Genie
This is how you can see if Urma Deal Genie is running and is successful or has errors. Remember it will take 5 minutes at least before you can see any results.
1. Go to [Lambda services](https://console.aws.amazon.com/lambda)
1. Click on the "UrmaDealGenie" function
1. Click on “Monitor” tab and look at "Metrics"
    - Watch the “Error count and success rate” chart
    - Green dots/line (Success) count should be 1
    - Red dots/line (Error) count should be 0
1. Click on "Logs" tab under Monitor
    - Click on a recent log file
    - Scroll down the log file and see if the logging shows the function is running and finding deals

**!!!Coming Soon!!! - Alarms and Monitor Dashboard**
![MonitorAlarms](https://user-images.githubusercontent.com/13062477/147112029-3d6466c1-664b-4d9b-9a80-b020201b4cc6.jpg)

## Testing Rules
It's useful to test rules without updating your deals. Here's how:
1. Go to [Lambda services](https://console.aws.amazon.com/lambda)
1. Click on the "UrmaDealGenie" function
1. Click on “Test” tab, create new Test event and save it
    - Saved event name = `deal-genie-update-false`
    - Copy / past body of `test-config-update-false.json`
    - Modify this JSON document - e.g. delete rules you don’t want, change deal rules include/exclude terms to be relative to your bot names
    - See [docs/ExampleConfigs.md](./docs/ExampleConfigs.md) for example configs with detailed explanations of rule settings
    - Click Save changes
    - Click Test and expand the Details and hopefully it runs!!
      - If it ran successfully, you can change UpdateDeals to “true” and see if your deals get updated. 
      - It is strongly recommended to modify the rules’ include/exclude terms fields so that the number of deals that need updating is just a few to start with.

# Console Application
This application requires .NET 6.0 Runtime:
  - [Windows x64 .NET 6.0 installer](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-6.0.0-windows-x64-installer)
  - [MacOS x64 .NET 6.0 installer](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-6.0.0-macos-x64-installer)
  - For other installers, go to [.NET 6.0 runtime](https://dotnet.microsoft.com/download/dotnet/6.0) and download appropriate **installer** under ".NET Runtime 6.0.0
" section
## Setup and Run
_First off, **SORRY** this is so complicated!! I'm working on making this one-click!_

To install and configure the Urma Deal Genie console application:
1. Download [UrmaDealGenieApp release files](https://github.com/UrmaGurd/UrmaDealGenie/releases/tag/app-2.3 ) depending on your operating system file to a local folder, e.g. `c:\UrmaDealGenieApp`
   - `dealrules.json`
   - `UrmaDealGenieApp-win10-x64.exe`, or `UrmaDealGenieApp-osx-x64` or `UrmaDealGenieApp-linux-x64`
4. Edit `dealrules.json` file on your machine to match your bots. See [docs/ExampleConfigs.md](./docs/ExampleConfigs.md) for example configs with detailed explanations of rule settings
5. Open a command/terminal window:
   - On Windows go to Start->Command Prompt
   - On Mac launch Terminal
   - On Linux... I dunno, google it?
   - navigate to the folder you downloaded the files to (e.g. `cd UrmaDealGenieApp`)
6. In the console/terminal window, set environment variables for the APIKEY and SECRET
   - Windows
     ```
     set APIKEY=YOUR_API_KEY_HERE
     set SECRET=YOUR_3COMMAS_SECRET_HERE
     ```
   - Mac/Linux
     ```
     export APIKEY=YOUR_API_KEY_HERE
     export SECRET=YOUR_3COMMAS_SECRET_HERE
     ```
5. Run UrmaDealGenieApp depending on your operating system
   - Windows
     ```
     UrmaDealGenieApp-win10-x64.exe
     ```
   - Mac
     ```
     UrmaDealGenieApp-osx-x64
     ```
   - Mac
     ```
     UrmaDealGenieApp-linux-x64
     ```
That's it. It will run in until you stop it by pressing Ctrl+C

# Run in a Docker Container
See [README-DOCKER.md](./docs/README-DOCKER.md)

