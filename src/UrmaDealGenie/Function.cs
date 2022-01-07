using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using UrmaDealGenie;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UrmaDealGenieLambda
{
  /// <summary>
  ///  The main AWS Lambda Function class and handler
  /// </summary>
  public class Function
  {
    private Urma3cClient client = null;

    /// <summary>
    /// Modify the TP % of all deals that have botnames that match the specified include/exclude terms.
    /// The TP is modified based on the modifier and the deal's completed SO count.
    /// Environment variables are used as follows:
    /// e.g. IncludeTerms = "urma,hodl" - to modify deals with botnames containing "urma" or "hodl"
    ///      ExcludeTerms = "btc,express bot,accum" - to NOT modify deals with botnames containing "btc" or "express bot" or "accum"
    ///      TpScale = 1.05 - to modify each matching deals' TP % to equal 1.05 x completed SO count
    ///      MaxSafetyOrderCount = modify deals less than max SO count
    ///      IgnoreTtpDeals = true - ignore deals with TTP enabled
    /// </summary>
    /// <param name="input">Input deal rules, or flag to trigger load from S3</param>
    /// <param name="context">Context of the lambda</param>
    /// <returns>Result summary of the updated deals</returns>
    public async Task<List<DealResponse>> FunctionHandler(DealRuleSet dealRuleSet, ILambdaContext context)
    {
      List<DealResponse> response = new List<DealResponse>();
      var bucket = Environment.GetEnvironmentVariable("BUCKET");
      Console.WriteLine($"LoadFromS3 = {dealRuleSet.LoadFromS3}");
      if (dealRuleSet.LoadFromS3)
      {
        // Load deal rules configuration from S3 bucket
        RegionEndpoint region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
        string dealRulesString = await BucketFileReader.ReadObjectDataAsync(region, bucket, "dealrules.json");
        if (!string.IsNullOrEmpty(dealRulesString))
        {
          dealRuleSet = JsonSerializer.Deserialize<DealRuleSet>(dealRulesString);
        }
        else
        {
          dealRuleSet = null;
        }
      }

      // By now we should have a deal rule set from S3 bucket or input parameter
      if (dealRuleSet != null)
      {
        client = new Urma3cClient();
        response = await client.ProcessRules(dealRuleSet);
      }
      else
      {
        Console.WriteLine($"Error: cannot find deal rules from input parameter or S3 bucket");
      }
      return response;
    }
  }
}
