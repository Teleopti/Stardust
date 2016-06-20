using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ConcatenatedTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		public bool IsTransient(Exception ex)
		{
			var microsoftTransientErrorDetection = new SqlDatabaseTransientErrorDetectionStrategy();
			var sqlTransientErrorChecker = new SqlTransientErrorChecker();
			return microsoftTransientErrorDetection.IsTransient(ex) || sqlTransientErrorChecker.IsTransient(ex);
		}
	}
}