using System.Text;
using System.Web;

namespace Teleopti.Ccc.TestCommon.Web
{
    public class FakeHttpResponse : HttpResponseBase
    {
        private readonly StringBuilder _outputString = new StringBuilder();

        public string ResponseOutput
        {
            get { return _outputString.ToString(); }
        }

        public override HttpCachePolicyBase Cache
        {
            get { return new FakeHttpCachePolicy(); }
        }

        public override int StatusCode { get; set; }

        public override string RedirectLocation { get; set; }

        public override void Write(string s)
        {
            _outputString.Append(s);
        }

        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }

	    public override bool TrySkipIisCustomErrors { get; set; }
    }

    public class FakeHttpCachePolicy : HttpCachePolicyBase
    {
    }
}