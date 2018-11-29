using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;


namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ManageSchedule
{
	public class ScheduleManagementTestBase
	{
		public IScheduleStorage ScheduleStorage;
		public IPersonRepository PersonRepository;
		public IScenarioRepository ScenarioRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IAgentDayScheduleTagRepository AgentDayScheduleTagRepository;
		public INoteRepository NoteRepository;
		public IPublicNoteRepository PublicNoteRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;
		public IScheduleTagRepository ScheduleTagRepository;
		public IAbsenceRepository AbsenceRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IActivityRepository ActivityRepository;
		public IJobResultRepository JobResultRepository;
		public WithUnitOfWork WithUnitOfWork;

		protected Scenario SourceScenario;
		protected Scenario TargetScenario;
		protected DateOnlyPeriod Period;
		protected IPerson Person;
		protected IBusinessUnit BusinessUnit;
		protected Guid JobResultId;

		protected void AddDefaultTypesToRepositories()
		{
			WithUnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(SourceScenario);
				ScenarioRepository.Add(TargetScenario);
				PersonRepository.Add(Person);
			});

			var jobResult = new JobResult(JobCategory.CopySchedule, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), Person, DateTime.UtcNow);
			WithUnitOfWork.Do(() =>
			{
				JobResultRepository.Add(jobResult);
			});
			JobResultId = jobResult.Id.GetValueOrDefault();
		}

		protected void VerifyCanBeFoundInScheduleStorageForTargetScenario(IPerson person)
		{
			var result = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(true, true),
				Period, TargetScenario));
			result[person].ScheduledDayCollection(Period).Should().Not.Be.Empty();
		}

		protected void VerifyJobResultIsUpdated(int numberOfDetails = 1)
		{
			WithUnitOfWork.Do(() =>
			{
				var jobResult = JobResultRepository.Get(JobResultId);
				jobResult.FinishedOk.Should().Be.True();
				jobResult.Details.Count(x => x.DetailLevel == DetailLevel.Info && x.ExceptionMessage == null).Should().Be(numberOfDetails);
			});
		}
	}
}