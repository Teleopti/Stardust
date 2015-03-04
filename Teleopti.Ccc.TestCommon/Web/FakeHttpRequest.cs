using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;

namespace Teleopti.Ccc.TestCommon.Web
{
    public class FakeHttpRequest : HttpRequestBase
    {
        private readonly HttpCookieCollection _cookies;
        private readonly NameValueCollection _formParams;
        private readonly NameValueCollection _queryStringParams;
        private readonly NameValueCollection _serverVariables;
        private readonly string _relativeUrl;
        private readonly Uri _url;
        private readonly Uri _urlReferrer;
        private readonly string _httpMethod;
	    private readonly NameValueCollection _headers;

	    public FakeHttpRequest(
		    string relativeUrl,
		    Uri url,
		    Uri urlReferrer)
		    : this(relativeUrl, "GET", url, urlReferrer, null, null, null)
	    {
	    }

	    public FakeHttpRequest(
		    string relativeUrl,
		    string method,
		    Uri url,
		    Uri urlReferrer,
		    NameValueCollection formParams,
		    NameValueCollection queryStringParams,
		    HttpCookieCollection cookies)
		    : this(relativeUrl, method, formParams, queryStringParams, cookies, null)
	    {
		    _url = url;
		    _urlReferrer = urlReferrer;
	    }

		public FakeHttpRequest(
			string relativeUrl,
			string method,
			NameValueCollection formParams,
			NameValueCollection queryStringParams,
			HttpCookieCollection cookies,
			NameValueCollection headers)
		{
			_httpMethod = method;
			_relativeUrl = relativeUrl;
			_formParams = formParams;
			_queryStringParams = queryStringParams;
			_cookies = cookies;
			_serverVariables = new NameValueCollection();
			_headers = headers ?? new NameValueCollection();
		}

	    public override NameValueCollection ServerVariables
	    {
		    get { return _serverVariables; }
	    }

	    public override NameValueCollection Headers
	    {
			get { return _headers; }
	    }

	    public override NameValueCollection Form
        {
            get { return _formParams; }
        }

        public override NameValueCollection QueryString
        {
            get { return _queryStringParams; }
        }

        public override HttpCookieCollection Cookies
        {
            get { return _cookies; }
        }

	    public override RequestContext RequestContext
	    {
		    get { return new RequestContext(); }
	    }

	    public override string AppRelativeCurrentExecutionFilePath
        {
            get { return _relativeUrl; }
        }

	    public override Uri Url
	    {
		    get { return _url; }
	    }

	    public override Uri UrlReferrer
	    {
		    get { return _urlReferrer; }
	    }

	    public override string PathInfo
        {
            get { return String.Empty; }
        }

	    public override string ApplicationPath
	    {
		    get { return ""; }
	    }

	    public override string HttpMethod
	    {
		    get { return _httpMethod; }
	    }

	    public override string UserHostAddress
	    {
		    get { return "172.168.1.1"; }
	    }

    }
}