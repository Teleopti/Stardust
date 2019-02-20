using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class FullDayAbsenceConfigurable : IUserDataSetup
	{
		public string Scenario { get; set; }
		public string Name { get; set; }
		public DateTime Date { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var scenario = new ScenarioRepository(unitOfWork).LoadAll().Single(abs => abs.Description.Name.Equals(Scenario));
			var absence = new AbsenceRepository(unitOfWork).LoadAll().Single(abs => abs.Description.Name.Equals(Name));
			var personAssignmentRepository = new PersonAssignmentRepository(unitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(unitOfWork);
			var noteRepository = new NoteRepository(unitOfWork);
			var publicNoteRepository = new PublicNoteRepository(unitOfWork);
			var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(unitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(unitOfWork);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(unitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(unitOfWork);
			var scheduleRepository = new ScheduleStorage(unitOfWork, personAssignmentRepository,
				personAbsenceRepository, new MeetingRepository(unitOfWork),
				agentDayScheduleTagRepository, noteRepository,
				publicNoteRepository, preferenceDayRepository,
				studentAvailabilityDayRepository, new PersonAvailabilityRepository(unitOfWork),
				new PersonRotationRepository(unitOfWork), overtimeAvailabilityRepository,
				new PersistableScheduleDataPermissionChecker(new FullPermission()),
				new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
					() => personAbsenceRepository,
					() => preferenceDayRepository, () => noteRepository,
					() => publicNoteRepository,
					() => studentAvailabilityDayRepository,
					() => agentDayScheduleTagRepository,
					() => overtimeAvailabilityRepository), new FullPermission());
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			var scheduleDifferenceSaver = new SaveSchedulePartService(new ScheduleDifferenceSaver(new EmptyScheduleDayDifferenceSaver(), new PersistScheduleChanges(scheduleRepository, CurrentUnitOfWork.Make())), personAbsenceAccountRepository, new DoNothingScheduleDayChangeCallBack());
			var businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(personAbsenceAccountRepository, new SchedulingResultStateHolder());
			var personAbsenceCreator = new NoScheduleChangedEventPersonAbsenceCreator(scheduleDifferenceSaver, businessRulesForAccountUpdate);
			var commandConverter = new AbsenceCommandConverter(new ThisCurrentScenario(scenario), new PersonRepository(unitOfWork, null, null), new AbsenceRepository(unitOfWork), scheduleRepository, UserTimeZone.Make());
			var handler = new AddFullDayAbsenceCommandHandler(personAbsenceCreator, commandConverter);
			handler.Handle(new AddFullDayAbsenceCommand
			{
				AbsenceId = absence.Id.Value,
				StartDate = Date,
				EndDate = Date,
				PersonId = person.Id.Value
			});
		}
	}

	internal class NoScheduleChangedEventPersonAbsenceCreator : PersonAbsenceCreator, IPersonAbsenceCreator
	{
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

		public NoScheduleChangedEventPersonAbsenceCreator(ISaveSchedulePartService saveSchedulePartService,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate) : base(saveSchedulePartService, businessRulesForPersonalAccountUpdate)
		{
			_saveSchedulePartService = saveSchedulePartService;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
		}

		public new IList<string> Create(AbsenceCreatorInfo absenceCreatorInfo, bool isFullDayAbsence, bool muteEvent = false)
		{
			var businessRulesForPersonAccountUpdate = _businessRulesForPersonalAccountUpdate.FromScheduleRange(absenceCreatorInfo.ScheduleRange);
			createPersonAbsence(absenceCreatorInfo);
			var ruleCheckResult = _saveSchedulePartService.Save(absenceCreatorInfo.ScheduleDay, businessRulesForPersonAccountUpdate, KeepOriginalScheduleTag.Instance);
			return ruleCheckResult;
		}

		private static void createPersonAbsence(AbsenceCreatorInfo absenceCreatorInfo)
		{
			var absenceLayer = new AbsenceLayer(
				absenceCreatorInfo.Absence,
				new DateTimePeriod(absenceCreatorInfo.AbsenceTimePeriod.StartDateTime,
					absenceCreatorInfo.AbsenceTimePeriod.EndDateTime));

			absenceCreatorInfo.ScheduleDay.CreateAndAddAbsence(absenceLayer);
		}
	}
}