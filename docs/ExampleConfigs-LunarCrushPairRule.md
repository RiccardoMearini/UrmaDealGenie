# LunarCrush Bot Pair Rules examples
Currently LunarCrush does not need an account or user specific API key to work. This may change in 2022 Q2 when LunarCrush changes their API.

NOTE: It is strongly recommended to test settings with the `"Update": false` value set before you set it to true and update your deals. Check the logging and make sure the pairs it wants to update your bot with matches what you expect.

Common behaviour for this rule across settings:
- only pairs on the bot's exchange will be returned
- all pairs returned will match the base pair of your bot (e.g. if your bot is BUSD_SOL, BUSD_DOGE, BUSD_LUNA, then it knows your base is BUSD)
- default max LunarCrush metric ranking is 1500, so no pairs with a rank higher will be returned


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
- it defines a BotId (one per rule) and you can specify Single or MultiPair (composite) bots
- the Metric is the text "Altrank" which tells Urma Deal Genie to get Altrank data ordered by ascending rank 
- there is a MaxPairCount of 10 so only that many coin pairs will be returned

## Example 2 - LunarCrush Altrank restricted by Black
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
- it defines a BotId (one per rule) and you can specify Single or MultiPair (composite) bots
- the Metric is the text "Altrank" which tells Urma Deal Genie to get Altrank data ordered by ascending rank 
- there is a MaxPairCount of 10 so only that many coin pairs will be returned
- the rule will make sure only pairs on the bot's exchange will be returned, and using the base pair your bot is using
