using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using UrmaDealGenie;

namespace UrmaDealGenie.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async void TestToUpperFunction()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var dealResponses = await function.FunctionHandler(new DealRuleSet(), context);

            Assert.Equal(2, dealResponses.Count);
        }
    }
}
