using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class PublicNoteOnWednesday : IUserDataSetup
	{
		public string PublicNote = "my public note";
		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var today = DateOnly.Today;
			var firstDay = today.AddDays(-7);
			var lastDay = today.AddDays(7);

			var period = new DateOnlyPeriod(firstDay, lastDay);
			var dayCollection = period.DayCollection();
			var publicNoteRepository = new PublicNoteRepository(uow);

			foreach (var dateOnly in dayCollection)
			{
				if (dateOnly.DayOfWeek != DayOfWeek.Tuesday) continue;

				publicNoteRepository.Add(new PublicNote(user, dateOnly, Scenario, PublicNote));
			}
		}
	}
}