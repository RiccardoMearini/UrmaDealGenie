# Safety Order Ranges Deal Rules examples
NOTE: It is strongly recommended to test settings with the `"Update": false` value set before you set it to true and update your deals. Make sure the number of deals the function says it thinks needs updating matches what you expect.

## Example 1 - Set a specific Take Profit for each Safety Order
This is the simplest use of this rule type, a directy mapping between Safety Order and Take Profit.

### Goal
Modify the TP% of each matching deal according to a lookup. Each SO will have a TP. This rule assumes there are the same number of safety orders as the deal's bot defines in max safety order count. 
### Useful for
If you have a bot that increases volume to cover a big drop, but when you hit the last couple of safety orders you actually want to get out of the deal quicker so reduce the TP.
### Configuration
```
{
  "Update": true,
  "SafetyOrderRangesDealRules": [
    {
      "Rule": "Bots with 5 safeties",
      "BotNameIncludeTerms": "RedFive",
      "BotNameExcludeTerms": "",
      "IgnoreTtpDeals": false,
      "AllowTpReduction": true,
      "SafetyOrderRanges": {
        "0": 0.5,
        "1": 1,
        "2": 2,
        "3": 3,
        "4": 4,
        "5": 0.5
      }
    }
  ]
}
```

### Explanation of settings
- `Update` - deals will be updated
- there is only 1 rule under the `SafetyOrderRangesDealRules` section that is named `Bots with 5 safeties` (name it as you wish)
- the rule type is `SafetyOrderRangesDealRules` which tells Urma Deal Genie to lookup the TP based on the deal's current SO count
- it defines 2 **include** terms `RedFive` and `hodl` which means the rule will match any deal that has a bot name like "My redfive" or "BTC RedFive Bot"
- deals that have TTP enabled are not excluded because `IgnoreTtpDeals` is set to `false`
- matching deals will be allowed to have their TP% reduced because `AllowTpReduction` is set to false
- the lookup of SO to TP is defined in the `SafetyOrderRanges` array
  - each lookup entry has the SO in quotes, a colon, and the TP as a decimal number (e.g. `"5": 1.5` means SO 5 will be set to 1.5% TP)
  - each entry is comma delimited (last entry has no comma after it)
  - make sure there are no duplicate SO entries
  - make sure the entries are numeric
  - make sure SO entries are whole numbers in quotes
  - if a deal has a higher SO count than in the lookup (e.g. 6 or more SOs in this example), the Deal Genie will lookup the highest SO it finds (e.g. 5 in this example) 
