using System;
using System.Collections.Generic;
using System.Threading;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IAuthorizationScope
	{
		IDisposable OnThisThreadUse(IAuthorization authorization);
	}

	public class ThisAuthorization : ICurrentAuthorization
	{
		private readonly IAuthorization _authorization;

		public ThisAuthorization(IAuthorization authorization)
		{
			_authorization = authorization;
		}

		public IAuthorization Current()
		{
			return _authorization;
		}
	}

	public class CurrentAuthorization : ICurrentAuthorization, IAuthorizationScope
	{
		private readonly IAuthorization _authorization;
		private static readonly ThreadLocal<Stack<IAuthorization>> _threadAuthorization = new ThreadLocal<Stack<IAuthorization>>(() => new Stack<IAuthorization>());

		public CurrentAuthorization(IAuthorization authorization)
		{
			_authorization = authorization;
		}

		public static CurrentAuthorization Make()
		{
			return new CurrentAuthorization(new PrincipalAuthorization(CurrentTeleoptiPrincipal.Make()));
		}
		
		public static IDisposable ThreadlyUse(IAuthorization authorization)
		{
			_threadAuthorization.Value.Push(authorization);
			return new GenericDisposable(() =>
			{
				_threadAuthorization.Value.Pop();
			});
		}

		public IDisposable OnThisThreadUse(IAuthorization authorization)
		{
			return ThreadlyUse(authorization);
		}

		public IAuthorization Current()
		{
			if (_threadAuthorization.Value.Count > 0)
				return _threadAuthorization.Value.Peek();
			return _authorization;
		}
	}
}