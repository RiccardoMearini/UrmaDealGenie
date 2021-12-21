using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UrmaDealGenie
{
  class BucketFileReader
  {
    public static async Task<string> ReadObjectDataAsync(RegionEndpoint region, string bucketName, string keyName)
    {
      IAmazonS3 client = new AmazonS3Client(region);
      string responseBody = "";
      Console.WriteLine($"Region {region}, bucket {bucketName}, key {keyName}");
      try
      {
        GetObjectRequest request = new GetObjectRequest
        {
          BucketName = bucketName,
          Key = keyName
        };
        using (GetObjectResponse response = await client.GetObjectAsync(request))
        using (Stream responseStream = response.ResponseStream)
        using (StreamReader reader = new StreamReader(responseStream))
        {
          string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
          string contentType = response.Headers["Content-Type"];
          Console.WriteLine("Object metadata, Title: {0}", title);
          Console.WriteLine("Content type: {0}", contentType);

          responseBody = reader.ReadToEnd(); // Now you process the response body.
        }
      }
      catch (AmazonS3Exception e)
      {
        // If bucket or object does not exist
        Console.WriteLine("AmazonS3Exception trying to read object: Message:'{0}' when reading object", e.Message);
        Console.WriteLine($"Check object {keyName} exists in bucket {bucketName}");
      }
      catch (Exception e)
      {
        Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading object", e.Message);
      }
      return responseBody;
    }
  }
}
