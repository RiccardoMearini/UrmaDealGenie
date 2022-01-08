using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UrmaDealGenie
{
  // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
  public class Btc
  {
    public int id { get; set; }
    public string s { get; set; }
    public string n { get; set; }
    public double p { get; set; }
    public int p_btc { get; set; }
    public double v { get; set; }
    public double vt { get; set; }
    public double pc { get; set; }
    public double pch { get; set; }
    public long mc { get; set; }
    public double gs { get; set; }
    public int ss { get; set; }
    public double @as { get; set; }
    public int bl { get; set; }
    public int br { get; set; }
    public int sp { get; set; }
    public int na { get; set; }
    public int md { get; set; }
    public int t { get; set; }
    public int r { get; set; }
    public int yt { get; set; }
    public int sv { get; set; }
    public int u { get; set; }
    public int c { get; set; }
    public double sd { get; set; }
    public double d { get; set; }
    public double cr { get; set; }
    public int acr { get; set; }
    public int tc { get; set; }
    public string categories { get; set; }
  }

  public class Config
  {
    public string data { get; set; }
    public string desc { get; set; }
    public int limit { get; set; }
    public string sort { get; set; }
    public string type { get; set; }
    public int page { get; set; }
    public int total_rows { get; set; }
    public Btc btc { get; set; }
  }

  public class Datum
  {
    public int id { get; set; }
    public string s { get; set; }
    public string n { get; set; }
    public double p { get; set; }
    public double? p_btc { get; set; }
    public double v { get; set; }
    public double vt { get; set; }
    public double pc { get; set; }
    public double pch { get; set; }
    public int mc { get; set; }
    public double gs { get; set; }
    public int ss { get; set; }
    public double @as { get; set; }
    public int bl { get; set; }
    public int br { get; set; }
    public int sp { get; set; }
    public int na { get; set; }
    public int md { get; set; }
    public int t { get; set; }
    public int r { get; set; }
    public int yt { get; set; }
    public int sv { get; set; }
    public int u { get; set; }
    public int? c { get; set; }
    public int sd { get; set; }
    public double d { get; set; }
    public double cr { get; set; }
    public int acr { get; set; }
    public int tc { get; set; }
    public string categories { get; set; }
    public int? e2 { get; set; }
    public int? df { get; set; }
    public int? nft { get; set; }
    public int? bsc { get; set; }
    public int? sc { get; set; }
    public int? dot { get; set; }
  }

  public class Root
  {
    public Config config { get; set; }
    public List<Datum> data { get; set; }
  }


}