using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LunarCrush.Objects
{
  // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
  public class Btc
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("s")]
    public string S { get; set; }

    [JsonPropertyName("n")]
    public string N { get; set; }

    [JsonPropertyName("p")]
    public double P { get; set; }

    [JsonPropertyName("p_btc")]
    public double PBtc { get; set; }

    [JsonPropertyName("v")]
    public double V { get; set; }

    [JsonPropertyName("vt")]
    public double Vt { get; set; }

    [JsonPropertyName("pc")]
    public double Pc { get; set; }

    [JsonPropertyName("pch")]
    public double Pch { get; set; }

    [JsonPropertyName("mc")]
    public long Mc { get; set; }

    [JsonPropertyName("gs")]
    public double Gs { get; set; }

    [JsonPropertyName("ss")]
    public int Ss { get; set; }

    [JsonPropertyName("as")]
    public double As { get; set; }

    [JsonPropertyName("bl")]
    public int Bl { get; set; }

    [JsonPropertyName("br")]
    public int Br { get; set; }

    [JsonPropertyName("sp")]
    public int Sp { get; set; }

    [JsonPropertyName("na")]
    public int Na { get; set; }

    [JsonPropertyName("md")]
    public int Md { get; set; }

    [JsonPropertyName("t")]
    public int T { get; set; }

    [JsonPropertyName("r")]
    public int R { get; set; }

    [JsonPropertyName("yt")]
    public int Yt { get; set; }

    [JsonPropertyName("sv")]
    public int Sv { get; set; }

    [JsonPropertyName("u")]
    public int U { get; set; }

    [JsonPropertyName("c")]
    public int C { get; set; }

    [JsonPropertyName("sd")]
    public double Sd { get; set; }

    [JsonPropertyName("d")]
    public double D { get; set; }

    [JsonPropertyName("cr")]
    public double Cr { get; set; }

    [JsonPropertyName("acr")]
    public int Acr { get; set; }

    [JsonPropertyName("tc")]
    public int Tc { get; set; }

    [JsonPropertyName("categories")]
    public string Categories { get; set; }
  }

  public class Config
  {
    [JsonPropertyName("data")]
    public string Data { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("sort")]
    public string Sort { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("total_rows")]
    public int TotalRows { get; set; }

    [JsonPropertyName("btc")]
    public Btc Btc { get; set; }
  }

  public class Usage
  {
    [JsonPropertyName("total")]
    public int Total { get; set; }
  }

  public class Datum
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("s")]
    public string S { get; set; }

    [JsonPropertyName("n")]
    public string N { get; set; }

    [JsonPropertyName("p")]
    public double P { get; set; }

    [JsonPropertyName("p_btc")]
    public double PBtc { get; set; }

    [JsonPropertyName("v")]
    public double V { get; set; }

    [JsonPropertyName("vt")]
    public double Vt { get; set; }

    [JsonPropertyName("pc")]
    public double Pc { get; set; }

    [JsonPropertyName("pch")]
    public double Pch { get; set; }

    [JsonPropertyName("mc")]
    public object Mc { get; set; }

    [JsonPropertyName("gs")]
    public double Gs { get; set; }

    [JsonPropertyName("ss")]
    public int Ss { get; set; }

    [JsonPropertyName("as")]
    public double As { get; set; }

    [JsonPropertyName("bl")]
    public int Bl { get; set; }

    [JsonPropertyName("br")]
    public int Br { get; set; }

    [JsonPropertyName("sp")]
    public int Sp { get; set; }

    [JsonPropertyName("na")]
    public int Na { get; set; }

    [JsonPropertyName("md")]
    public int Md { get; set; }

    [JsonPropertyName("t")]
    public int T { get; set; }

    [JsonPropertyName("r")]
    public int R { get; set; }

    [JsonPropertyName("yt")]
    public int Yt { get; set; }

    [JsonPropertyName("sv")]
    public int Sv { get; set; }

    [JsonPropertyName("u")]
    public int U { get; set; }

    [JsonPropertyName("c")]
    public int C { get; set; }

    [JsonPropertyName("sd")]
    public double Sd { get; set; }

    [JsonPropertyName("d")]
    public double D { get; set; }

    [JsonPropertyName("cr")]
    public double Cr { get; set; }

    [JsonPropertyName("acr")]
    public int Acr { get; set; }

    [JsonPropertyName("tc")]
    public int Tc { get; set; }

    [JsonPropertyName("categories")]
    public string Categories { get; set; }

    [JsonPropertyName("df")]
    public int Df { get; set; }

    [JsonPropertyName("e2")]
    public int E2 { get; set; }

    [JsonPropertyName("nft")]
    public int? Nft { get; set; }

    [JsonPropertyName("dot")]
    public int? Dot { get; set; }

    [JsonPropertyName("bsc")]
    public int? Bsc { get; set; }

    [JsonPropertyName("zil")]
    public int? Zil { get; set; }

    [JsonPropertyName("meme")]
    public int? Meme { get; set; }
  }

  public class Root
  {
    [JsonPropertyName("config")]
    public Config Config { get; set; }

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; }

    [JsonPropertyName("data")]
    public List<Datum> Data { get; set; }
  }

}