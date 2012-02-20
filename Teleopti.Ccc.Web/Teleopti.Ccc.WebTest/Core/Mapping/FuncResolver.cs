using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Core.Mapping
{
	public class FuncResolver<T> : IResolve<T>
	{
		private readonly Func<T> _resolver;

		public FuncResolver(Func<T> resolver) { _resolver = resolver; }

		public T Invoke() { return _resolver.Invoke(); }
	}

	public static class Resolver
	{
		public static IResolve<T> Of<T>(Func<T> resolver) { return new FuncResolver<T>(resolver); }
	}
}
