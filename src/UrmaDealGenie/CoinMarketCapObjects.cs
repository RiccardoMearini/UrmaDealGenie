using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoinMarketCap.Objects
{
  // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
  public class Status
  {
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public object ErrorMessage { get; set; }

    [JsonPropertyName("elapsed")]
    public int Elapsed { get; set; }

    [JsonPropertyName("credit_count")]
    public int CreditCount { get; set; }

    [JsonPropertyName("notice")]
    public object Notice { get; set; }

    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
  }

  public class Platform
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("token_address")]
    public string TokenAddress { get; set; }
  }

  public class BTC
  {
    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("volume_24h")]
    public double Volume24h { get; set; }

    [JsonPropertyName("volume_change_24h")]
    public double VolumeChange24h { get; set; }

    [JsonPropertyName("percent_change_1h")]
    public double PercentChange1h { get; set; }

    [JsonPropertyName("percent_change_24h")]
    public double PercentChange24h { get; set; }

    [JsonPropertyName("percent_change_7d")]
    public double PercentChange7d { get; set; }

    [JsonPropertyName("percent_change_30d")]
    public double PercentChange30d { get; set; }

    [JsonPropertyName("percent_change_60d")]
    public double PercentChange60d { get; set; }

    [JsonPropertyName("percent_change_90d")]
    public double PercentChange90d { get; set; }

    [JsonPropertyName("market_cap")]
    public double MarketCap { get; set; }

    [JsonPropertyName("market_cap_dominance")]
    public double MarketCapDominance { get; set; }

    [JsonPropertyName("fully_diluted_market_cap")]
    public double FullyDilutedMarketCap { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; set; }
  }

  public class Quote
  {
    [JsonPropertyName("BTC")]
    public BTC BTC { get; set; }
  }

  public class Datum
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public int NumMarketPairs { get; set; }

    [JsonPropertyName("date_added")]
    public DateTime DateAdded { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("max_supply")]
    public long? MaxSupply { get; set; }

    [JsonPropertyName("circulating_supply")]
    public double CirculatingSupply { get; set; }

    [JsonPropertyName("total_supply")]
    public double TotalSupply { get; set; }

    [JsonPropertyName("is_active")]
    public int IsActive { get; set; }

    [JsonPropertyName("platform")]
    public Platform Platform { get; set; }

    [JsonPropertyName("cmc_rank")]
    public int CmcRank { get; set; }

    [JsonPropertyName("is_fiat")]
    public int IsFiat { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTime LastUpdated { get; set; }

    [JsonPropertyName("quote")]
    public Quote Quote { get; set; }
  }

  public class Root
  {
    [JsonPropertyName("status")]
    public Status Status { get; set; }

    [JsonPropertyName("data")]
    public List<Datum> Data { get; set; }
  }

}