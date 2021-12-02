# Scaling Take Profit Deal Rules examples
NOTE: It is strongly recommended to test settings with the `"UpdateDeals": false` value set before you set it to true and update your deals. Make sure the number of deals the function says it thinks needs updating matches what you expect.

## Example 1 - Set Take Profit to match the Safety Order count
This is the original reason I created Urma Deal Genie, so that I can simply increase the TP% to match the deal's current SO count. An ["Urma" bot](UrmaBotSettings.md) is budget DCA bot that costs as little as $250 and covers a 60% drop using 7 Safety Orders (SO).

### Configuration
```
{
  "UpdateDeals": false,
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
This example builds on the settings in the previous example, but enables deal update and has a 2nd deal rule called "BTC accumulator". Both rules are of the same type `ScalingTakeProfitDealRules`.

### Configuration
```
{
  "UpdateDeals": true,
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
      "AllowTpReduction": false,
      "MaxSafetyOrderCount": 4,
      "TpScale": 0.5
    }
  ]
}
```
### Explanation of settings
The 2nd rule defines 1 **include** terms `btc` which means the rule will match any deal that has a bot name containing that term.
So it will match bots called "BTC HODL" and also bots called "Urma BTC".

However there is 1 **exclude** term `urma` which means the rule will not match bots called "Urma BTC", so this exclude **overrides** the include.

With `MaxSafetyOrderCount` set to `4` the rule will stop altering matching deals that have already surpassed 4 SOs. So the rule will not apply to SO 5, 6, 7 etc.

And finally there is scaling applied to the TP%, because `TpScale` is set to `0.5`, which means TP% is set to SO count x 0.5, e.g. for SO 3 the TP% will be set to 1.5.

