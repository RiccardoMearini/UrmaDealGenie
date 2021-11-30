using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using UrmaDealGenie;
using System.IO;

namespace UrmaDealGenie.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async void TestFunctionCall()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var text = File.ReadAllText("dealrules.json");
            Console.WriteLine($"{text}");
            var input = JsonSerializer.Deserialize<DealRuleSet>(text);
            var dealResponses = await function.FunctionHandler(input, context);

            Assert.Equal(5, dealResponses.Count);
        }
    }
}
