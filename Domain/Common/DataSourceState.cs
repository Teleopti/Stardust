using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class DataSourceState
	{
		[ThreadStatic]
		public static IDataSource ThreadDataSource;
	}
}