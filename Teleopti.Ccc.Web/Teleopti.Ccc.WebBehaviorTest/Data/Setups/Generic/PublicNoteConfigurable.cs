using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PublicNoteConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public string NoteText { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;
			var publicNoteRepository = new PublicNoteRepository(uow);
			publicNoteRepository.Add(new PublicNote(user, new DateOnly(Date), scenario, NoteText));
		}
	}
}