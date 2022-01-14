using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UrmaDealGenie
{
  /// <summary>
  ///  The main AWS Lambda Function class and handler
  /// </summary>
  public class Function
  {
    private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { 
      WriteIndented = true,
      Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } 
    };

    private Urma3cClient client = null;

    /// <summary>
    /// Create UrmaDealGenie 3Commas Client and process deal rules, which are either
    /// passed in as complete DealRuleSet parameter or loaded from S3 bucket
    /// </summary>
    /// <param name="dealRuleSet">Input deal rules, OR just a dealRuleSet.LoadFromS3 flag</param>
    /// <param name="context">Context of the lambda - not used</param>
    /// <returns>Result summary of the updated deals</returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(DealRuleSet dealRuleSet, ILambdaContext context)
    {
      int returnCode = (int)HttpStatusCode.OK;
      string returnBody = "";

      var apiKey = Environment.GetEnvironmentVariable("APIKEY");
      var secret = Environment.GetEnvironmentVariable("SECRET");
      var bucket = Environment.GetEnvironmentVariable("BUCKET");

      if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secret))
      {
        returnCode = (int)HttpStatusCode.BadRequest;
        returnBody = $"ERROR {returnCode}: Missing APIKEY and/or SECRET environment variables";
        Console.WriteLine($"{returnBody}");
      }
      else
      {
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

        Console.WriteLine($"LoadFromS3 = {dealRuleSet.LoadFromS3}");
        List<DealResponse> response = new List<DealResponse>();

        // By now we should have a deal rule set from S3 bucket or input parameter
        if (dealRuleSet == null)
        {
          returnCode = (int)HttpStatusCode.BadRequest;
          returnBody = $"ERROR {returnCode}: Cannot find deal rules from input parameter or S3 bucket";
          Console.WriteLine($"{returnBody}");
        }
        else
        {
          // We're all set, crack on with the Urma Deal Genie!!
          client = new Urma3cClient(apiKey, secret);
          response = await client.ProcessRules(dealRuleSet);
          returnBody = JsonSerializer.Serialize<List<DealResponse>>(response, this.jsonOptions);
        }
      }

      return new APIGatewayProxyResponse
      {
        StatusCode = returnCode,
        Body = returnBody,
      };
    }
  }
}
