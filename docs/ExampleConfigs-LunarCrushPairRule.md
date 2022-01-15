# LunarCrush Pair Rule
This rule changes a bot's pairs according to the top ranked coins on LunarCrush, either the Altrank or GalaxyScore. 

**NOTE:** Currently LunarCrush does not need an account or user specific API key to work. This may change in 2022 Q2 when LunarCrush changes their API.

## How to use this rule
- Create a single or multipair bot in 3Commas (paper or real) and decide it's settings, start conditions etc.
- Set your Max Active Deals (if multipair) and give the bot some random pairs (of same base pair, e.g. BUSD, USDT etc) to start with (the rule will overwrite them)
- Save but DO NO START the bot
- Now get the `BotId` for the bot you want to change pairs - this is the 8 digit BotId from the address bar when you edit a bot in 3Commas.
- Create a `LunarCrushPairRules` rule in dealrules.json as per examples below
- Run UrmaDealGenie with this rule at least once so that it updates the pairs
- Check the bot in 3Commas to see the pairs are updated
- Start the bot!

## What the rule does
Each time the rule is run (default 5 minutes) the rule will:
- find coins up to the MaxPairCount (or your bot's Max Active Deals, which ever is higher)
- it will NOT update Max Active Deals on your bot, it will only update the pairs
- it finds coins that are not stablecoins, not in the 3commas blacklist and not in the rule's blacklist
- all pairs returned will match the base pair of your bot (e.g. if your bot is BUSD_SOL, BUSD_DOGE, BUSD_LUNA, then it knows your base is BUSD)
- only pairs on your bot's exchange will be returned
- it will not return pairs that are higher than the MaxMetricScore (default is 1500)
- if you have a CoinMarketCap API key, you can specify that in the UrmaDealGenie installation or environment settings, and the UrmaDealGenie will not return pairs that are higher than the MaxCmcRank (default is 200, which is the free credit threshold in CoinMarketCap API)

# LunarCrush Bot Pair Rules examples

NOTE: It is strongly recommended to test settings with the `"Update": false` value set before you set it to true and update your deals. Check the logging and make sure the pairs it wants to update your bot with matches what you expect.

## Example 1 - LunarCrush Altrank 
This example is using a bot that is in a paper trade account. You get the 8 digit BotId from the address bar when you edit a bot in 3Commas.

### Goal
To find the top 10 LunarCrush Altrank pairs, and set a bot's pairs to use these.

### Useful for
Selecting trending coins. Consider running a bot that has RSI and TradingView buy start conditions so that you don't get caught at the peak of a pump.

### Configuration
```
{
  "Update": true,
  "LunarCrushPairRules": [
    {
      "Rule": "Urma with Start Conditions - Paper",
      "BotId": 12345678,
      "Metric": "Altrank",
      "MaxPairCount": 10
    }
  ]
}
```

### Explanation of settings
- `Update` - deals will be updated
- there is only 1 rule under the `LunarCrushPairRules` section that is named `Urma with Start Conditions - Paper` (name it as you wish)
- the rule type is `LunarCrushPairRules` which tells Urma Deal Genie to find LunarCrush data
- it defines a `BotId` (one per rule) and you can specify Single or MultiPair (composite) bots
- the `Metric` is the text `"Altrank"` which tells Urma Deal Genie to get Altrank data ordered by ascending rank (best first)
- there is a `MaxPairCount` of 10 so only that many coin pairs will be returned

## Example 2 - LunarCrush GalaxyScore restricted by blacklist and CoinMarketCap rank
This example is using a bot that is in a paper trade account.

### Goal
To find the top 5 LunarCrush Altrank pairs, excluding coins in specific coin blacklist, and are within the top 100 coins on CoinMarketCap, and set a bot's pairs to use these.

### Useful for
Selecting safer trending coins that you are more likely comfortable holding if the bot gets a red bag.

### Configuration
```
{
  "Update": true,
  "LunarCrushPairRules": [
    {
      "Rule": "Urma GalaxyScore - Paper",
      "BotId": 7778278,
      "Metric": "GalaxyScore",
      "MaxCmcRank": 100,
      "BlacklistCoins": "BTC,ETH,BNB,ADA,XRP,DOGE,SHIB,COMP",
      "MaxPairCount": 5
    }
  ]
}
```

### Explanation of settings
- `Update` - deals will be updated
- there is only 1 rule under the `LunarCrushPairRules` section that is named `Urma GalaxyScore - Paper` (name it as you wish)
- the rule type is `LunarCrushPairRules` which tells Urma Deal Genie to find LunarCrush data
- it defines a `BotId` (one per rule) and you can specify Single or MultiPair (composite) bots
- the `Metric` is the text `"GalaxyScore"` which tells Urma Deal Genie to get GalaxyScore data ordered by descending GalaxyScore rank (best first)
- the `BlacklistCoins` ensures the comma separated coin token names will not be returned by this rule, in addition to your 3Commas blacklist
- there is a `MaxPairCount` of 5 so only that many coin pairs will be returned
