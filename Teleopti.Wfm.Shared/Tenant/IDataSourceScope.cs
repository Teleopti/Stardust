using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IDataSourceScope
	{
		IDisposable OnThisThreadUse(IDataSource dataSource);
		IDisposable OnThisThreadUse(string tenant);
	}
}