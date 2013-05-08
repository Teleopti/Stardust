using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public sealed class FakeDataSource : IDataSource
	{
		public IUnitOfWorkFactory Statistic { get; set; }
		public IUnitOfWorkFactory Application { get; set; }
		public IAuthenticationSettings AuthenticationSettings { get; set; }
		public string DataSourceName { get; set; }

		public void ResetStatistic()
		{
			throw new NotImplementedException();
		}
		public string OriginalFileName { get; set; }
		public AuthenticationTypeOption AuthenticationTypeOption { get; set; }

		public void Dispose()
		{
		}
	}
}