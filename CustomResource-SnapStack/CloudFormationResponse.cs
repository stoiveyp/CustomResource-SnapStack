using System;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomResource_SnapStack
{
    public class CloudFormationResponse
    {
        public string Status { get; set; }
        public string Reason { get; set; }
        public string PhysicalResourceId { get; set; }
        public string StackId { get; set; }
        public string RequestId { get; set; }
        public string LogicalResourceId { get; set; }
        public object Data { get; set; }

        public static async Task<CloudFormationResponse> CompleteCloudFormationResponse(object data, CloudFormationRequest request, ILambdaContext context)
        {
            data = data ?? new JObject();
            var responseBody = new CloudFormationResponse
            {
                Status = data is Exception ? "FAILED" : "SUCCESS",
                Reason = "See the details in CloudWatch Log Stream: " + context.LogStreamName,
                PhysicalResourceId = context.LogStreamName,
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                Data = data is Exception ? new JObject(new JProperty("exception:", data.ToString())) : data
            };

            try
            {
                using (var client = new HttpClient())
                {

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(responseBody));
                    jsonContent.Headers.Remove("Content-Type");

                    var postResponse = await client.PutAsync(request.ResponseURL, jsonContent);

                    postResponse.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                LambdaLogger.Log("Exception: " + ex);

                responseBody.Status = "FAILED";
                responseBody.Data = new JObject(new JProperty("exception:", ex.ToString()));
            }

            return responseBody;
        }
    }
}