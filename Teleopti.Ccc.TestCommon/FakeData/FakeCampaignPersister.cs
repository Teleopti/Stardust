using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeCampaignPersister : ICampaignPersister
	{
		public void Persist(IScenario scenario, IWorkload workload, DateOnly[] days, double campaignTasksPercent)
		{
			throw new NotImplementedException();
		}
	}
}