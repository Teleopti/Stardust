using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public class EtlReadModelSetup: IDataSetup
	{
		public IBusinessUnit BusinessUnit;
		public string StepName;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var d = new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc);
			var rep = new EtlReadModelRepository(currentUnitOfWork.Current());
			rep.LastChangedDate(BusinessUnit, StepName,new DateTimePeriod(d.AddDays(-10),d.AddDays(10)));
		}
	}
}