using System;
using System.Threading;
using System.Web;
using Teleopti.Ccc.Domain2;

namespace Teleopti.Ccc.Infrastructure.Web
{
	public class CurrentHttpContext : ICurrentHttpContext
	{
		private static HttpContextBase globalContext;
		private static readonly ThreadLocal<HttpContextBase> threadContext = new ThreadLocal<HttpContextBase>();

		public IDisposable GloballyUse(HttpContextBase context)
		{
			globalContext = context;
			return new GenericDisposable(() => { globalContext = null; });
		}

		public IDisposable OnThisThreadUse(HttpContextBase context)
		{
			threadContext.Value = context;
			return new GenericDisposable(() => { threadContext.Value = null; });
		}

		public HttpContextBase Current()
		{
			if (globalContext != null)
				return globalContext;
			if (threadContext.Value != null)
				return threadContext.Value;
			if (HttpContext.Current != null)
				return new HttpContextWrapper(HttpContext.Current);
			return null;
		}
	}
}