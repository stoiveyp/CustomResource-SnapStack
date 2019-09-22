using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CustomResource_SnapStack
{
    public class Function
    {
        public async Task<CloudFormationResponse> FunctionHandler(SnapStackRequest input, ILambdaContext context)
        {
            if (string.Equals("Delete", input.RequestType, StringComparison.OrdinalIgnoreCase))
            {
                return await CloudFormationResponse.CompleteCloudFormationResponse(null, input, context);
            }

            return await SnapshotStackResources(input, context);
        }

        private async Task<CloudFormationResponse> SnapshotStackResources(SnapStackRequest input, ILambdaContext context)
        {
            try
            {
                var stackName = input.StackId.Split('/')[1];
                var results = await GetStackResource(input,stackName);
                await StoreResultsInDynamo(results, input.ResourceProperties, stackName);
                return await CloudFormationResponse.CompleteCloudFormationResponse(null, input, context);
            }
            catch (Exception ex)
            {
                return await CloudFormationResponse.CompleteCloudFormationResponse(ex, input, context);
            }
        }

        private async Task StoreResultsInDynamo(string results, SnapStackResourceProperties properties, string stackName)
        {
            var dynamo = new AmazonDynamoDBClient(RegionEndpoint.EUWest1);
            var item = new Dictionary<string, AttributeValue>
            {
                {"stack", new AttributeValue(stackName)},
                {"version", new AttributeValue(properties.SortKey)},
                {"data", new AttributeValue(results)}
            };
            var response = await dynamo.PutItemAsync(properties.Table, item);
        }

        private async Task<string> GetStackResource(SnapStackRequest input, string stackName)
        {
            var cfClient = new AmazonCloudFormationClient(RegionEndpoint.EUWest1);
            

            //TODO: Paginate
            var request = new ListStackResourcesRequest
            {
                StackName = stackName
            };
            var json = new JObject();
            var resourceList = await cfClient.ListStackResourcesAsync(request);
            foreach (var resource in resourceList.StackResourceSummaries)
            {
                json.Add($"{resource.ResourceType}_{resource.LogicalResourceId}", resource.PhysicalResourceId);
            }

            return json.ToString(Formatting.Indented);
        }
    }
}
