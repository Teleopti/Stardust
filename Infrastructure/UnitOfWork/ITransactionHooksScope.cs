using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ITransactionHooksScope
	{
		IDisposable GloballyUse(IEnumerable<ITransactionHook> messageSenders);
		IDisposable OnThisThreadUse(IEnumerable<ITransactionHook> messageSenders);
		IDisposable OnThisThreadExclude<T>();
	}
}