using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PersonDayOffConfigurable : IUserDataSetup
	{
		public string Name { get; set; }
		public DateTime Date { get; set; }

		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var dayOff = new DayOffRepository(uow).LoadAll().Single(dayOffTemplate => dayOffTemplate.Description.Name.Equals(Name));
			var personDayOff = new PersonDayOff(user, Scenario, dayOff, new DateOnly(Date));

			var repository = new PersonDayOffRepository(uow);
			repository.Add(personDayOff);
		}
	}
}
