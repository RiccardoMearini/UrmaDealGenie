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
You can deploy Urma Deal Genie as: 
- an AWS Lambda Function (serverless)
- a .NET Core application that runs on Mac/Linux/Windows
- a Docker container

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

# 3Commas DCA Bots
If you haven't already, go checkout [TradeAlt's Trading Bots](https://youtu.be/ziy-9yYTrbc) tutorial YouTube playlist. It will get you setup with DCA trading bots with a safe and effective passive income trading bot setup. I cannot stress enough how important this guy's video series is! 

The Urma Deal Genie works with DCA bot deals, and uses bot names to determine which deals to apply rules to. So try to name your bots with meaningful names that represent the strategy of that bot. e.g. "TA Safer BUSD" or "BTC HODL" or "Urma 250"

Checkout the [Urma DCA bot settings](docs/UrmaBotSettings.md) for a low budget, higher profit alternative to TradeAlt's settings.

# Create a 3Commas API key and secret
The Urma Deal Genie needs to connect to your 3Commas account, and it needs 
1. Go to https://3commas.io/api_access_tokens and click "New API access token" 
1. Give it a name like "UrmaDealGenie"
1. Tick "Bots read" and "Bots write"
1. Take a note of the API Key and Secret, you'll need them later in the instructions

# Install Instructions for UrmaDealGenie
There are several ways to run UrmaDealGenie, but the original and preferred way is Amazon Web Services (AWS):
- [Amazon Web Services](./docs/README-AWS.md) (for free!)
- [Console Application](./docs/README-CONSOLE-APPLICATION.md) on Windows, Mac OSX or Linux
- [Docker Container](./docs/README-DOCKER.md) for how to run UrmaDealGenie in a Docker container

## Deployment Diagrams
![Urma Deal Genie deployment diagrams](https://user-images.githubusercontent.com/13062477/147111837-617c9ed1-47a8-43ef-a338-c40e96e5d582.png)

