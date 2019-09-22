using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CustomResource_SnapStack
{
    public class SnapStackRequest:CloudFormationRequest
    {
        [JsonProperty("ResourceProperties")]
        public SnapStackResourceProperties ResourceProperties { get; set; }
    }
}
