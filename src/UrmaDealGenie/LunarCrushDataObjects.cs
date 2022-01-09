using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UrmaDealGenie
{
  public class Root
  {
    [JsonPropertyName("config")]
    public Config Config { get; set; }

    [JsonPropertyName("data")]
    public List<Datum> Data { get; set; }
  }

  public class Btc
  {
    [JsonPropertyName("id")]
    public double Id { get; set; }

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
    public double Ss { get; set; }

    [JsonPropertyName("as")]
    public double As { get; set; }

    [JsonPropertyName("bl")]
    public double Bl { get; set; }

    [JsonPropertyName("br")]
    public double Br { get; set; }

    [JsonPropertyName("sp")]
    public double Sp { get; set; }

    [JsonPropertyName("na")]
    public double Na { get; set; }

    [JsonPropertyName("md")]
    public double Md { get; set; }

    [JsonPropertyName("t")]
    public double T { get; set; }

    [JsonPropertyName("r")]
    public double R { get; set; }

    [JsonPropertyName("yt")]
    public double Yt { get; set; }

    [JsonPropertyName("sv")]
    public double Sv { get; set; }

    [JsonPropertyName("u")]
    public double U { get; set; }

    [JsonPropertyName("c")]
    public double C { get; set; }

    [JsonPropertyName("sd")]
    public double Sd { get; set; }

    [JsonPropertyName("d")]
    public double D { get; set; }

    [JsonPropertyName("cr")]
    public double Cr { get; set; }

    [JsonPropertyName("acr")]
    public double Acr { get; set; }

    [JsonPropertyName("tc")]
    public double Tc { get; set; }

    [JsonPropertyName("categories")]
    public string Categories { get; set; }
  }

  public class Config
  {
    [JsonPropertyName("data")]
    public string Data { get; set; }

    [JsonPropertyName("desc")]
    public string Desc { get; set; }

    [JsonPropertyName("limit")]
    public double Limit { get; set; }

    [JsonPropertyName("sort")]
    public string Sort { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("page")]
    public double Page { get; set; }

    [JsonPropertyName("total_rows")]
    public double TotalRows { get; set; }

    [JsonPropertyName("btc")]
    public Btc Btc { get; set; }
  }

  public class Datum
  {
    [JsonPropertyName("id")]
    public double Id { get; set; }

    [JsonPropertyName("s")]
    public string S { get; set; }

    [JsonPropertyName("n")]
    public string N { get; set; }

    [JsonPropertyName("p")]
    public double P { get; set; }

    [JsonPropertyName("p_btc")]
    public double? PBtc { get; set; }

    [JsonPropertyName("v")]
    public double V { get; set; }

    [JsonPropertyName("vt")]
    public double Vt { get; set; }

    [JsonPropertyName("pc")]
    public double Pc { get; set; }

    [JsonPropertyName("pch")]
    public double Pch { get; set; }

    [JsonPropertyName("mc")]
    public double Mc { get; set; }

    [JsonPropertyName("gs")]
    public double Gs { get; set; }

    [JsonPropertyName("ss")]
    public double Ss { get; set; }

    [JsonPropertyName("as")]
    public double As { get; set; }

    [JsonPropertyName("bl")]
    public double Bl { get; set; }

    [JsonPropertyName("br")]
    public double Br { get; set; }

    [JsonPropertyName("sp")]
    public double Sp { get; set; }

    [JsonPropertyName("na")]
    public double Na { get; set; }

    [JsonPropertyName("md")]
    public double Md { get; set; }

    [JsonPropertyName("t")]
    public double T { get; set; }

    [JsonPropertyName("r")]
    public double R { get; set; }

    [JsonPropertyName("yt")]
    public double Yt { get; set; }

    [JsonPropertyName("sv")]
    public double Sv { get; set; }

    [JsonPropertyName("u")]
    public double U { get; set; }

    [JsonPropertyName("c")]
    public double? C { get; set; }

    [JsonPropertyName("sd")]
    public double Sd { get; set; }

    [JsonPropertyName("d")]
    public double D { get; set; }

    [JsonPropertyName("acr")]
    public double Acr { get; set; }

    [JsonPropertyName("tc")]
    public double Tc { get; set; }

    [JsonPropertyName("categories")]
    public string Categories { get; set; }

    [JsonPropertyName("bsc")]
    public double Bsc { get; set; }

    [JsonPropertyName("df")]
    public double? Df { get; set; }

    [JsonPropertyName("nft")]
    public double? Nft { get; set; }

    [JsonPropertyName("cr")]
    public double? Cr { get; set; }

    [JsonPropertyName("e2")]
    public double? E2 { get; set; }

    [JsonPropertyName("zil")]
    public double? Zil { get; set; }
  }
}