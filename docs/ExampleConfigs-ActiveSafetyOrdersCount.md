# Active Safety Orders Count Ranges Deal Rules examples
NOTE: It is strongly recommended to test settings with the `"UpdateDeals": false` value set before you set it to true and update your deals. Make sure the number of deals the function says it thinks needs updating matches what you expect.

## Example 1 - Reduce the Active Safety Orders Count
This is the main use of this rule type, to reduce the Active Safety Orders Count as safety orders are filled.

### Goal
To reduce the Active Safety Orders Count (aka Max Active Safety Trade Count MASTC) when the deal reaches a given safety order. This assumes the deals are created with MASTC of 2 or more.
### Useful for
When price crashes so fast that 3C can't put in orders fast enough. So lets say price drops 10% in seconds, one SO is filled but the next one doesn't get placed in time and price has already dropped past it. The idea of having multiple active SOs (MASTC > 1) is to safeguard against that.

HOWEVER, the flip side is if you have MASTC set to 2 or more, and you are running anything over 100% risk, even 101%, then you can end up in a position where funds are locked into large volume orders for the bottom most SOs in a deal, and some deals will fail to place their last orders. Which can mess up your DCA.

This deal rule gives the best of both worlds, but only if your Step Scaling is > 1.

## Known Issue
tl;dr - It works, but not exactly as you expect.</br>
Although 3Commas supports modifying a deal's MASTC, the settings doesn't take effect immediately. It only takes effect on _future_ safety orders. So if you start deals with MASTC set to 2, and then when SO 3 is reached you set it to 1, 3Commas will leave SO 4 intact, and then when SO4 is hit, only 1 active safety order will be opened. The effect is the same, but don't be surprised if you see your deal summary view still reporting they have 2 MASTC when in fact it's set to 1 on the deal.
### Configuration
```
{
  "UpdateDeals": true,
  "ActiveSafetyOrdersCountRangesDealRules": [
    {
      "Rule": "Urma MASTC 2 to 1",
      "BotNameIncludeTerms": "urma",
      "BotNameExcludeTerms": "",
      "IgnoreTtpDeals": true,
      "ActiveSafetyOrdersCountRanges": {
        "1": 2,
        "3": 1
      }
    }
  ],
  "SafetyOrderRangesDealRules": [ ],
  "ScalingTakeProfitDealRules": [ ]
}
```

### Explanation of settings
- `UpdateDeals` - deals will be updated
- there is only 1 rule under the `ActiveSafetyOrdersCountRangesDealRules` section that is named `Urma MASTC 2 to 1` (name it as you wish)
- the rule type is `ActiveSafetyOrdersCountRangesDealRules` which tells Urma Deal Genie to lookup the MASTC based on the deal's current SO count
- it defines 1 **include** terms `urma` which means the rule will match any deal that has a bot name like "My Urma Deals"
- deals that have TTP enabled are excluded because `IgnoreTtpDeals` is set to `true`
- the lookup of SO to MASTC is defined in the `ActiveSafetyOrdersCountRanges` array
  - each lookup entry has the SO in quotes, a colon, and the MASTC as a while number (e.g. `"1": 2` means SO 1 will be set to 2 MASTC)
  - each entry is comma delimited (last entry has no comma after it)
  - make sure there are no duplicate SO entries
  - make sure the entries are numeric
  - make sure SO entries are whole numbers in quotes
  - if a deal has a higher SO count than in the lookup (e.g. 4 or more SOs in this example), the Deal Genie will lookup the highest SO it finds (e.g. 3 in this example) 
