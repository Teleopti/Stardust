﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using System.Web.SessionState;

namespace Teleopti.Ccc.TestCommon.Web
{
	public class FakeHttpContext : HttpContextBase
	{
		private readonly HttpCookieCollection _cookies;
		private readonly NameValueCollection _formParams;
		private IPrincipal _principal;
		private readonly NameValueCollection _queryStringParams;
		private readonly string _relativeUrl;
		private readonly string _method;
		private readonly SessionStateItemCollection _sessionItems;
		private HttpResponseBase _response;
		private HttpRequestBase _request;
		private readonly Dictionary<object, object> _items;
		private NameValueCollection _headers;

		public FakeHttpContext()
			: this("~/", null, null, null, null, null, null)
		{
		}

		public FakeHttpContext(string relativeUrl, string method)
			: this(relativeUrl, method, null, null, null, null, null)
		{
		}

		public FakeHttpContext(string relativeUrl) 
			: this(relativeUrl, null, null, null, null, null)
		{
		}

		public FakeHttpContext(string relativeUrl, IPrincipal principal, NameValueCollection formParams,
							   NameValueCollection queryStringParams, HttpCookieCollection cookies,
							   SessionStateItemCollection sessionItems) 
			: this(relativeUrl, null, principal, formParams, queryStringParams, cookies, sessionItems)
		{
		}

		public FakeHttpContext(string relativeUrl, string method, IPrincipal principal, NameValueCollection formParams,
							   NameValueCollection queryStringParams, HttpCookieCollection cookies,
							   SessionStateItemCollection sessionItems)
			: this(relativeUrl, null, principal, formParams, queryStringParams, cookies, sessionItems, null)
		{
		}

		public FakeHttpContext(string relativeUrl, string method, IPrincipal principal, NameValueCollection formParams,
							   NameValueCollection queryStringParams, HttpCookieCollection cookies,
							   SessionStateItemCollection sessionItems, NameValueCollection headers)
		{
			_relativeUrl = relativeUrl;
			_method = method;
			_principal = principal;
			_formParams = formParams;
			_queryStringParams = queryStringParams;
			_cookies = cookies;
			_sessionItems = sessionItems;
			_headers = headers;

			_items = new Dictionary<object, object>();
		}

		public override HttpRequestBase Request
		{
			get
			{
				return _request ?? (_request = new FakeHttpRequest(_relativeUrl, _method, _formParams, _queryStringParams, _cookies, _headers));
			}
		}

		public void SetRequest(HttpRequestBase request)
		{
			_request = request;
		}

		public override HttpResponseBase Response
		{
			get
			{
				return _response ?? (_response = new FakeHttpResponse());
			}
		}

		public override HttpServerUtilityBase Server
		{
			get { return new FakeHttpServerUtility(); }
		}

		public override HttpApplicationStateBase Application
		{
			get { return new FakeHttpApplicationState(); }
		}

		public void SetResponse(HttpResponseBase response)
		{
			_response = response;
		}

		public override IPrincipal User
		{
			get { return _principal; }
			set { _principal = value; }
		}

		public override HttpSessionStateBase Session
		{
			get { return new FakeHttpSessionState(_sessionItems); }
		}

		public override System.Collections.IDictionary Items
		{
			get
			{
				return _items;
			}
		}

		public void AddItem(object key, object value)
		{
			_items.Add(key, value);
		}

		public override bool SkipAuthorization { get; set; }

		public override object GetService(Type serviceType)
		{
			return null;
		}
	}

	public class FakeHttpApplicationState : HttpApplicationStateBase
	{
	}

	public class FakeHttpServerUtility : HttpServerUtilityBase
	{
	}
}