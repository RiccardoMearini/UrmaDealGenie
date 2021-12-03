# Example Deal Genie Configurations

## General Information on Deal Rules
- Deal rules are specified in JSON format, and [must be valid](https://jsonlint.com/)
- Each rule is applied in the order defined in the JSON file
- Rules are applied in 2 parts
  - finding matching deals based on a set of criteria
  - applying changes to deals (e.g. the TP)
- Multiple rules could be applied to the same deal, but this isn't recommended

## Main Structure of Configuration File
The JSON snippet below shows the overall structure of a configuration. 
```
{
  "UpdateDeals": false,
  "ScalingTakeProfitDealRules": [ ... ],
  "SafetyOrderRangesDealRules": [ ... ]
}
```
There is a set of {} braces around the entire config, with at least 3 sections:
- UpdateDeals can be true or false.
  - true means the deals are updated as per the rule criteria
  - false means the deals that would be updated are shown but not updated. This is useful for testing your rules first.
- [ScalingTakeProfitDealRules](ExampleConfigs-ScalingTakeProfits.md) is a rule type section that contains 1 or more rules of the same type
- [SafetyOrderRangesDealRules](ExampleConfigs-SafetyOrderRanges.md) is a rule type section that contains 1 or more rules of the same type
- You must have at least 1 rule of any type

## Defining Multiple Rules
Each rule type section has rules within it.
- the rules are defined within the square braces `[ ]` of the rule section
- each rule is defined within a set of curly braces `{ }`
- multiple rules can be defined, and there must be a comma between them (typically after the first rule's final curly brace}
- the last rule within a rule section must not have a comma after it
- the `...` in the examples below indicated snipped out content, this is for illustration only
```
{
  "UpdateDeals": false,
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
  ]
}
```
## Examples by Rule Type
- [Scaling Take Profit Deal Rules](ExampleConfigs-ScalingTakeProfits.md)
- [Safety Order Ranges Deal Rules](ExampleConfigs-SafetyOrderRanges.md)

