using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class DayOffScheduled : IUserDataSetup
	{

		private readonly int _weekday;
		public IDayOffTemplate DayOffTemplate = TestData.DayOffTemplate;

		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public DayOffScheduled(int weekday)
		{
			_weekday = weekday;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var dayOffRepository = new DayOffRepository(uow);
			dayOffRepository.Add(DayOffTemplate);
		}
	}
}