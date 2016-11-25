using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Archiving;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Archiving
{
	[Category("BucketB")]
	[Toggle(Toggles.Wfm_ImportSchedule_41247)]
	[DatabaseTest]
	public class ImportScheduleHandlerTest : ISetup
	{
		public ImportScheduleHandler Target;

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

		//private Scenario _defaultScenario;
		//private Scenario _targetScenario;
		//private DateOnlyPeriod _archivePeriod;
		//private IPerson _person;
		//private IBusinessUnit _businessUnit;
		//private Guid _jobResultId;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
		}

		[Test]
		public void Test()
		{
			Target.Handle(new ImportScheduleEvent());
		}
	}
}