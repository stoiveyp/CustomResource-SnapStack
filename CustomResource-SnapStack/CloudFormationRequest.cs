namespace CustomResource_SnapStack
{
    public class CloudFormationRequest
    {
        public string StackId { get; set; }
        public string ResponseURL { get; set; }
        public string RequestType { get; set; }
        public string ResourceType { get; set; }
        public string RequestId { get; set; }
        public string LogicalResourceId { get; set; }
    }
}
