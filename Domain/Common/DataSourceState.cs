using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class DataSourceState
	{
		[ThreadStatic]
		private static IDataSource _threadDataSource;

		public void SetOnThread(IDataSource dataSource)
		{
			_threadDataSource = dataSource;
		}

		public IDataSource Get()
		{
			return _threadDataSource;
		}

	}
}