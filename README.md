# Urma-Gurd's 3Commas Deal Genie
This is an Amazon Web Services (AWS) Lambda Function that runs regularly in the cloud, and updates configured 3Commas deals to change the Take Profit % based on sets of rules.

It requires an AWS account, and some basic knowledge of AWS would be useful. Although AWS requires a credit card to create an account, it will not charge you if you only use free tier services, and fortunately Lambda functions are part of the [AWS Free Tier](https://aws.amazon.com/free/). 

# Getting Started
You will need:
1. A 3Commas account that is setup with at least 1 active DCA bot
3. An Amazon Web Services account
4. Latest UrmaDealGenie.zip package

## 3Commas DCA Bots
If you haven't already, go checkout [TradeAlt's Trading Bots](https://youtu.be/ziy-9yYTrbc) tutorial YouTube playlist. It will get you setup with DCA trading bots with a safe and effective passive income trading bot setup. I cannot stress enough how important this guy's video series is! 

The Urma Deal Genie works with DCA bot deals, and uses bot names to determine which deals to apply rules to. So try to name your bots with meaningful names that represent the strategy of that bot. e.g. "TA Safer BUSD" or "BTC HODL" or "Urma 250"

### Create a 3Commas API key and secret
The Urma Deal Genie needs to connect to your 3Commas account, and it needs 
1. Go to https://3commas.io/api_access_tokens and click "New API access token" 
1. Give it a name like "UrmaDealGenie"
1. Tick Bots read, Bots write, Accounts read, Smart trades read

## Create an AWS Account

