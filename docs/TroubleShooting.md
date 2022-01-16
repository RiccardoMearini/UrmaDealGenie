# General Issues

## GetExchange fails due to Invalid 3C API 
If you see the following error:
```
Unhandled exception. System.ArgumentNullException: Value cannot be null. (Parameter 'array')
   at System.Array.Find[T](T[] array, Predicate`1 match)
   at UrmaDealGenie.LunarCrushPairRuleProcessor.GetExchange(Int32 accountId)
```
It's usually because your 3Commas API key is invalid or missing permissions. Try creating a new API key with **BotRead**, **BotWrite** and **AccountRead** permissions

# AWS Issues
See [here for AWS Issues](README-AWS.md#troubleshooting)

# Console Application Issues
TBD
