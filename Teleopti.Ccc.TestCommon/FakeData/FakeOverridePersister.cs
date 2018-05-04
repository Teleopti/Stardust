using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeOverridePersister : IOverridePersister
	{
		public void Persist(IScenario scenario, IWorkload workload, OverrideInput input)
		{
			throw new NotImplementedException();
		}
	}
}