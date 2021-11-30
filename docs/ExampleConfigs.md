# Example Urma Deal Genie Configurations
These example configurations can be used in the EventBridge JSON content input, or in the Lambda Test payloads.

It is strongly recommended to test settings with the `"UpdateDeals": false` value set before you set it to true and update your deals. Make sure the number of deals the function says it thinks needs updating matches what you expect.

## Scale Urma bots TP% = SO count
This is the original reason I created Urma Deal Genie, so that I can simply increase the TP% to match the deal's current SO count.

An "Urma" bot is budget DCA bot that costs as little as $250 and covers a 60% drop using 7 Safety Orders (SO).

So in this configuration, there is 1 rule that I named "Urma SO scaling" (name it as you wish).

The rule type is `ScaleTp` which tells Urma Deal Genie that to scale the TP% based on the SO count and the `TpScale` factor.

The rule defines 2 **include** terms `urma` and `hodl` which means the rule will match any deal that has a bot name containing EITHER of those two terms. Each term is comma delimited.
So it will match bots called "Urma BUSD $250" and also bots called "My HODL bot".

However there is 1 **exclude** term `btc` which means the rule will not match bots called "Urma BTC HODL" or "My BTC bot".

Deals that have TTP enabled will be ignored because `IgnoreTtpDeals` is set to `true`.

There is no maximum SO count because `MaxSafetyOrderCount` is set to `0`. So the rule will apply to all SOs for the bot (in this case, 1-7)

And finally there is no scaling applied to the TP%, because `TpScale` is set to `1`, which means TP% is simply set to the SO count.

```
{
  "UpdateDeals": true,
  "DealRules": [
    {
      "Rule": "Urma SO scaling",
      "BotNameIncludeTerms": "urma,hodl",
      "BotNameExcludeTerms": "btc",
      "IgnoreTtpDeals": true,
      "MaxSafetyOrderCount": 0,
      "TpScale": 1
    }
  ]
}
```

## Multiple Deal Rules, with TP scaling
Building on the settings above, this config has a 2nd deal rule called "BTC accumulator".

The 2nd rule defines 1 **include** terms `btc` which means the rule will match any deal that has a bot name containing that term.
So it will match bots called "BTC HODL" and also bots called "Urma BTC".

However there is 1 **exclude** term `urma` which means the rule will not match bots called "Urma BTC", so this exclude **overrides** the include.

With `MaxSafetyOrderCount` set to `4` the rule will stop altering matching deals that have already surpassed 4 SOs. So the rule will not apply to SO 5, 6, 7 etc.

And finally there is scaling applied to the TP%, because `TpScale` is set to `0.5`, which means TP% is set to SO count x 0.5, e.g. for SO 3 the TP% will be set to 1.5.

```
{
  "UpdateDeals": false,
  "DealRules": [
    {
      "Rule": "Urma SO scaling",
      "BotNameIncludeTerms": "urma,hodl",
      "BotNameExcludeTerms": "btc",
      "IgnoreTtpDeals": true,
      "MaxSafetyOrderCount": 0,
      "TpScale": 1
    },
    {
      "Rule": "BTC accumulator",
      "BotNameIncludeTerms": "btc",
      "BotNameExcludeTerms": "urma",
      "IgnoreTtpDeals": true,
      "MaxSafetyOrderCount": 4,
      "TpScale": 0.5
    }
  ]
}
```
