using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ITransactionHooksScope
	{
		IDisposable OnThisThreadExclude<T>();
	}
}