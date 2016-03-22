using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Stardust.Manager.Extensions;

namespace Stardust.Manager
{
	public class RetryPolicyProvider
	{
		private const int _delaysMiliseconds = 100;
		private const int _maxRetry = 3;

		public RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> GetPolicy()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(_delaysMiliseconds);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(_maxRetry, fromMilliseconds);
			return policy;
		}
	}
}