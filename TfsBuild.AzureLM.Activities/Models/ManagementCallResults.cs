using System.Net;
using System.Xml.Linq;

namespace TfsBuild.AzureLM.Activities.Models
{
    public class ManagementApiResults
    {
        public XDocument ResponseBody { get; set; }
        public string RequestId { get; set; }
        public bool ResultCodeMatch { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
