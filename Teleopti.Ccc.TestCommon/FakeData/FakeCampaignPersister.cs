using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCampaignPersister : ICampaignPersister
	{
		public void Persist(IScenario scenario, IWorkload workload, ModifiedDay[] days, double campaignTasksPercent)
		{
			throw new NotImplementedException();
		}
	}
}