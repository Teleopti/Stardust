using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IDataSourceScope
	{
		IDisposable OnThisThreadUse(IDataSource dataSource);
		IDisposable OnThisThreadUse(string tenant);
	}
}