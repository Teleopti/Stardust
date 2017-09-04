using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class FullDayAbsenceConfigurable : IUserDataSetup
	{
		public string Scenario { get; set; }
		public string Name { get; set; }
		public DateTime Date { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var scenario = new ScenarioRepository(currentUnitOfWork).LoadAll().Single(abs => abs.Description.Name.Equals(Scenario));
			var absence = new AbsenceRepository(currentUnitOfWork).LoadAll().Single(abs => abs.Description.Name.Equals(Name));
			var repositoryFactory = new RepositoryFactory();
			var scheduleRepository = new ScheduleStorage(currentUnitOfWork, repositoryFactory, new PersistableScheduleDataPermissionChecker(), new ScheduleStorageRepositoryWrapper(repositoryFactory, currentUnitOfWork));
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			var scheduleDifferenceSaver = new SaveSchedulePartService(new ScheduleDifferenceSaver(scheduleRepository, CurrentUnitOfWork.Make(), new EmptyScheduleDayDifferenceSaver()), personAbsenceAccountRepository, new DoNothingScheduleDayChangeCallBack());
			var businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository, new SchedulingResultStateHolder());
			var personAbsenceCreator = new PersonAbsenceCreator (scheduleDifferenceSaver, businessRulesForAccountUpdate);
			var commandConverter = new AbsenceCommandConverter(new ThisCurrentScenario(scenario), new PersonRepository(currentUnitOfWork), new AbsenceRepository(currentUnitOfWork), scheduleRepository, UserTimeZone.Make());
			var handler = new AddFullDayAbsenceCommandHandler(personAbsenceCreator, commandConverter);
			handler.Handle(new AddFullDayAbsenceCommand
				{
					AbsenceId = absence.Id.Value,
					StartDate = Date,
					EndDate = Date,
					PersonId = user.Id.Value
				});
		}
	}
}