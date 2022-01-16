# Example Deal Genie Configurations

## General Information on Deal Rules
- Deal rules are specified in JSON format, and [must be valid](https://jsonlint.com/)
- Each rule is applied in the order defined in the JSON file
- Rules are applied in 2 parts
  - finding matching deals/bots based on a set of criteria
  - applying changes to deals/bots (e.g. change the deal's TP or bot's pairs)p
- Multiple rules could be applied to the same deal, e.g. LunarCrush rule to select pairs, and Safety Order Ranges to change the TP%)
- Most of the configuration is made up of key/value pairs
  - these pairs are separated by a colon `:`
  - keys are on the left side of a colons, and enclosed in quotes `"`
  - values are on the right side and can be with or without quotes (numbers and booleans without, text with)
  - boolean values must be either `true` of `false`

## Main Structure of Configuration File
The JSON snippet below shows the overall structure of a configuration. 
```
{
  "Update": false,
  "LunarCrushPairRules": [ ... ],  
  "ActiveSafetyOrdersCountRangesDealRules": [ ... ],  
  "ScalingTakeProfitDealRules": [ ... ],
  "SafetyOrderRangesDealRules": [ ... ]
}
```
There is a set of {} braces around the entire config, with at least 1 section:
- `Update` can be true or false.
  - true means the deals are updated as per the rule criteria
  - false means the deals that would be updated are shown but not updated. This is useful for testing your rules first.
- [LunarCrushPairRules](ExampleConfigs-LunarCrushPairRules.md) is a bot rule type section that contains 1 or more rules of the same type
- [ScalingTakeProfitDealRules](ExampleConfigs-ScalingTakeProfits.md) is a deal rule type section that contains 1 or more rules of the same type
- [SafetyOrderRangesDealRules](ExampleConfigs-SafetyOrderRanges.md) is a deal rule type section that contains 1 or more rules of the same type
- [ActiveSafetyOrdersCountRangesDealRules](ExampleConfigs-ActiveSafetyOrdersCount.md) is a deal rule type section that contains 1 or more rultes of the same type
- You can omit a rule type section completely if you are not using that rule.

## Defining Multiple Rules
Each rule type section has rules within it.
- the rules are defined within the square braces `[ ]` of the rule section
- each rule is defined within a set of curly braces `{ }`
- multiple rules can be defined, and there must be a comma between them (typically after the first rule's final curly brace}
- the last rule within a rule section must not have a comma after it
- the `...` in the examples below indicate snipped out content, this is for illustration only
```
{
  "Update": false,
  "LunarCrushPairRules": [
    {
      "Rule": "My LunarCrush rule",
      ...
    },
    {
      "Rule": "Another rule that uses LunarCrush",
      ...
    }
  ],
  "ScalingTakeProfitDealRules": [
    {
      "Rule": "My first scaling rule",
      ...
    },
    {
      "Rule": "Another rule that scales TP",
      ...
    }
  ],
  "SafetyOrderRangesDealRules": [
    {
      "Rule": "A SO ranges and TPs",
      ...
    },
    {
      "Rule": "Some rule for TP lookups",
      ...
    }
  ],
  "ActiveSafetyOrdersCountRangesDealRules": [
    {
      "Rule": "MASTC reduction rule 1",
      ...
    },
    {
      "Rule": "MASTC reduction rule 2",
      ...
    }
  ]
}
```
## Examples by Rule Type
- [LunarCrushPairRules](ExampleConfigs-LunarCrushPairRules.md)
- [Scaling Take Profit Deal Rules](ExampleConfigs-ScalingTakeProfits.md)
- [Safety Order Ranges Deal Rules](ExampleConfigs-SafetyOrderRanges.md)
- [ActiveSafetyOrdersCountRangesDealRules](ExampleConfigs-ActiveSafetyOrdersCount.md) 
