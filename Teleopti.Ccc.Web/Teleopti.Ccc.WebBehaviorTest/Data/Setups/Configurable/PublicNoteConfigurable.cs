using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PublicNoteConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public string NoteText { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = DefaultScenario.Scenario;
			var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
			publicNoteRepository.Add(new PublicNote(user, new DateOnly(Date), scenario, NoteText));
		}
	}
}