# Example Deal Genie Configurations

## General Information on Deal Rules
- Deal rules are specified in JSON format, and [must be valid](https://jsonlint.com/)
- Each rule is applied in the order defined in the JSON file
- Rules are applied in 2 parts
  - finding matching deals based on a set of criteria
  - applying changes to deals (e.g. the TP)
- Multiple rules could be applied to the same deal, but this isn't recommended

## Examples by Rule Type
- [Scaling Take Profit Deal Rules](ExampleConfigs-ScalingTakeProfits.md)
- [Safety Order Ranges Deal Rules](ExampleConfigs-SafetyOrderRanges.md)

