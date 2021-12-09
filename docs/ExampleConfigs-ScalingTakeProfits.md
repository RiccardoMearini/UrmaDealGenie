# Scaling Take Profit Deal Rules examples
NOTE: It is strongly recommended to test settings with the `"UpdateDeals": false` value set before you set it to true and update your deals. Make sure the number of deals the function says it thinks needs updating matches what you expect.

## Example 1 - Set Take Profit to match the Safety Order count
This is the original reason I created Urma Deal Genie, so that I could simple change the TP based on the SO count, and it works really well with Urma settings. An ["Urma" bot](UrmaBotSettings.md) is budget DCA bot that costs as little as $250 and covers a 60% drop using 7 Safety Orders (SO).

### Goal
Modify the TP% of each matching deal so that the TP% equals the SO count number. e.g. 1% for SO1, 2% for SO2 etc up to 7% for SO7.

### Configuration
```
{
  "UpdateDeals": false,
  "SafetyOrderRangesDealRules": [ ],
  "ScalingTakeProfitDealRules": [
    {
      "Rule": "Urma SO scaling",
      "BotNameIncludeTerms": "urma,hodl",
      "BotNameExcludeTerms": "btc",
      "IgnoreTtpDeals": true,
      "AllowTpReduction": false,
      "MaxSafetyOrderCount": 0,
      "TpScale": 1
    }
  ]
}
```
### Explanation of settings
- `UpdateDeals` - deals will not be updated, just a summary shown of deals that would be affected
- there is only 1 rule under the `ScalingTakeProfitDealRules` section that is named `Urma SO scaling` (name it as you wish)
- the rule type is `ScalingTakeProfitDealRules` which tells Urma Deal Genie to scale the TP% based on the SO count and the `TpScale` factor
- it defines 2 **include** terms `urma` and `hodl` which means the rule will match any deal that has a bot name containing EITHER of those two terms. Each term is comma delimited.<br/>
So it will match bots called "Urma BUSD $250" and also bots called "My HODL bot"
- there is 1 **exclude** term `btc` which means the rule will not match bots called "Urma BTC HODL" or "My BTC bot"
- deals that have TTP enabled will be ignored because `IgnoreTtpDeals` is set to `true`
- matching deals will not have their TP% reduced (e.g. if deal was manually set to higher TP) because `AllowTpReduction` is set to false
- the rule will apply to all SOs for matching deals (in this case, 1-7) because `MaxSafetyOrderCount` is set to `0`
- no scaling applied to the TP%, because `TpScale` is set to `1`, which means TP% is simply set to equal the SO count

## Example 2 - Multiple Rules that Scale Take Profit 
This example builds on the settings in the previous example but enables deal updates, and adds a 2nd deal rule called "BTC accumulator". Both rules are of the same type `ScalingTakeProfitDealRules`.

### Goal
Run 2 deal rules to modify 2 different types of deals at the same time:
- First rule is as above, set TP to match the SO count for deals with "urma" or "hodl" in their bot names
- Second rule is meant to only apply to BTC accumulator bots so that it increases the TP% a small amount for the first 4 safety orders only

### Configuration
```
{
  "UpdateDeals": true,
  "SafetyOrderRangesDealRules": [ ],
  "ScalingTakeProfitDealRules": [
    {
      "Rule": "Urma SO scaling",
      "BotNameIncludeTerms": "urma,hodl",
      "BotNameExcludeTerms": "btc",
      "IgnoreTtpDeals": true,
      "AllowTpReduction": false,
      "MaxSafetyOrderCount": 0,
      "TpScale": 1
    },
    {
      "Rule": "BTC accumulator",
      "BotNameIncludeTerms": "btc",
      "BotNameExcludeTerms": "urma",
      "IgnoreTtpDeals": true,
      "AllowTpReduction": true,
      "MaxSafetyOrderCount": 4,
      "TpScale": 0.5
    }
  ]
}
```
### Explanation of settings
- `UpdateDeals` - deals will be updated
- there are 2 rules under the `ScalingTakeProfitDealRules` section, each with a different rule name
- second rule defines **include** term `btc` and **exclude** term `urma` which means the rule will match bots called "BTC HODL" but not called "Urma BTC".
- deals that have TTP enabled will be ignored because `IgnoreTtpDeals` is set to `true`
- matching deals will be allowed to have their TP% reduced because `AllowTpReduction` is set to true
- the rule will apply to the first 4 SOs only because `MaxSafetyOrderCount` is set to `4`
- the TP% is scaled by factor of `0.5` for each SO, which means TP% = SO count x 0.5<br/>e.g. for SO 3 the TP% will be set to 1.5

