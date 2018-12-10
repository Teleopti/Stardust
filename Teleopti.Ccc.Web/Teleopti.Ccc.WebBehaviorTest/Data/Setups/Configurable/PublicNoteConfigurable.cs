using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PublicNoteConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public string NoteText { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var scenario = DefaultScenario.Scenario;
			var publicNoteRepository = new PublicNoteRepository(unitOfWork);
			publicNoteRepository.Add(new PublicNote(person, new DateOnly(Date), scenario, NoteText));
		}
	}
}