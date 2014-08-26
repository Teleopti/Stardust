using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PublicNoteConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public string NoteText { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = DefaultScenario.Scenario;
			var publicNoteRepository = new PublicNoteRepository(uow);
			publicNoteRepository.Add(new PublicNote(user, new DateOnly(Date), scenario, NoteText));
		}
	}
}