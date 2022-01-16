# General Issues
These issues and trouble shooting tips are common across AWS, Windows, Mac, Linux and Docker.

## GetExchange fails due to Invalid 3C API 
If you see the following error:
```
Unhandled exception. System.ArgumentNullException: Value cannot be null. (Parameter 'array')
   at System.Array.Find[T](T[] array, Predicate`1 match)
   at UrmaDealGenie.LunarCrushPairRuleProcessor.GetExchange(Int32 accountId)
```
it's usually because your 3Commas API key is invalid or missing permissions. Try creating a new API key with **BotRead**, **BotWrite** and **AccountRead** permissions

## LunarCrush data can't be deserialized
If you see the following error:
```
GetLunarCrushData() - FAILED TO DESERIALISE: Data:
{"config":{"data":"market","desc":"true","limit":100,"sort":"gs","type":"fast","page":0,"total_rows":3478,"btc":
```
it's a random error caused by LunarCrush sending back data in an unexpected format. I'm gradually resolving these over time, but their API is not consistent. Fields are missing or change type etc. It's frustrating but not a lot I can do at moment.

Please report any instances of this to me and include the full JSON data.

# AWS Issues
See [here for AWS Issues](README-AWS.md#troubleshooting)

# Windows Console Application Issues
TBD

# Linux Console Application Issues
TBD

# Mac Console Application Issues
TBD

# Docker Issues
TBD
