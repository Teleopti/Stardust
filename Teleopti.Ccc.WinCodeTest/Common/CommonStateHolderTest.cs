using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class CommonStateHolderTest
    {
        private MockRepository _mockRep;
        private readonly IList<IAbsence> _absenceColl = new List<IAbsence>();
        private readonly IList<IActivity> _activityColl = new List<IActivity>();
        private readonly IList<IShiftCategory> _shiftCategoryColl = new List<IShiftCategory>();
        private readonly IList<IDayOffTemplate> _dayOffColl = new List<IDayOffTemplate>();
        private readonly IList<IContract> _contractList = new List<IContract>();
        private readonly ICollection<IContractSchedule> _contractScheduleColl = new List<IContractSchedule>();
		private readonly IList<IWorkflowControlSet> _workflowControlSets = new List<IWorkflowControlSet>(); 

        private CommonStateHolder _commonStateHolder;

        [SetUp]
        public void Setup()
        {
            _mockRep = new MockRepository();
        }

        [Test]
        public void CanLoadInstance()
        {
            _absenceColl.Add(AbsenceFactory.CreateAbsence("Sick"));
            _dayOffColl.Add(DayOffFactory.CreateDayOff(new Description("test2")));
            _activityColl.Add(ActivityFactory.CreateActivity("act2"));
            IActivity deleted = ActivityFactory.CreateActivity("deleted");
            deleted.SetDeleted();
            _activityColl.Add(deleted);
            _shiftCategoryColl.Add(ShiftCategoryFactory.CreateShiftCategory("act2"));
			_workflowControlSets.Add(_mockRep.StrictMock<IWorkflowControlSet>());

            var unitOfWork = _mockRep.DynamicMock<IUnitOfWork>();
            var repositoryFactory = _mockRep.StrictMock<IRepositoryFactory>();
            var absenceRepMock = _mockRep.StrictMock<IAbsenceRepository>();
            var dayOffRepMock = _mockRep.StrictMock<IDayOffTemplateRepository>();
            var activityRepMock = _mockRep.StrictMock<IActivityRepository>();
            var shiftCategoryRepMock = _mockRep.StrictMock<IShiftCategoryRepository>();
            var contractRepMock = _mockRep.StrictMock<IContractRepository>();
            var contractScheduleRepMock = _mockRep.StrictMock<IContractScheduleRepository>();
            var partTimePercentageRepository = _mockRep.DynamicMock<IPartTimePercentageRepository>();
            var scheduleTagRep = _mockRep.StrictMock<IScheduleTagRepository>();
	        var workflowControlSetRepository = _mockRep.StrictMock<IWorkflowControlSetRepository>();
	        var multi = _mockRep.DynamicMock<IMultiplicatorDefinitionSetRepository>();

            Expect.Call(repositoryFactory.CreateAbsenceRepository(unitOfWork)).Return(absenceRepMock);
            Expect.Call(repositoryFactory.CreateDayOffRepository(unitOfWork)).Return(dayOffRepMock);
            Expect.Call(repositoryFactory.CreateActivityRepository(unitOfWork)).Return(activityRepMock);
            Expect.Call(repositoryFactory.CreateShiftCategoryRepository(unitOfWork)).Return(shiftCategoryRepMock);
            Expect.Call(repositoryFactory.CreateContractRepository(unitOfWork)).Return(contractRepMock);
            Expect.Call(repositoryFactory.CreateContractScheduleRepository(unitOfWork)).Return(contractScheduleRepMock);
            Expect.Call(repositoryFactory.CreateScheduleTagRepository(unitOfWork)).Return(scheduleTagRep).Repeat.Once();
	        Expect.Call(repositoryFactory.CreateWorkflowControlSetRepository(unitOfWork)).Return(workflowControlSetRepository);
	        Expect.Call(repositoryFactory.CreatePartTimePercentageRepository(unitOfWork)).Return(partTimePercentageRepository);
	        Expect.Call(repositoryFactory.CreateMultiplicatorDefinitionSetRepository(unitOfWork)).Return(multi);
            Expect.Call(activityRepMock.LoadAll()).IgnoreArguments().Return(_activityColl);
            Expect.Call(dayOffRepMock.LoadAll()).IgnoreArguments().Return(_dayOffColl);
            Expect.Call(absenceRepMock.LoadAll()).IgnoreArguments().Return(_absenceColl);
            Expect.Call(shiftCategoryRepMock.FindAll()).IgnoreArguments().Return(_shiftCategoryColl);
            Expect.Call(contractRepMock.FindAllContractByDescription()).Return(_contractList);
            Expect.Call(contractScheduleRepMock.LoadAllAggregate()).Return(_contractScheduleColl);
            Expect.Call(scheduleTagRep.LoadAll()).Return(new List<IScheduleTag>()).Repeat.Once();
	        Expect.Call(workflowControlSetRepository.LoadAll()).Return(_workflowControlSets);
	        Expect.Call(multi.LoadAll()).Return(new List<IMultiplicatorDefinitionSet> {null});

            _mockRep.ReplayAll();

			_commonStateHolder = new CommonStateHolder(new DisableDeletedFilter(new ThisUnitOfWork(unitOfWork)));
            _commonStateHolder.LoadCommonStateHolder(repositoryFactory,unitOfWork);

            Assert.AreEqual(1, _commonStateHolder.Absences.Count());
            Assert.AreEqual(1, _commonStateHolder.ActiveAbsences.Count());
            Assert.AreEqual(2, _commonStateHolder.Activities.Count());
            Assert.AreEqual(1, _commonStateHolder.ActiveActivities.Count());
            Assert.AreEqual(1, _commonStateHolder.DayOffs.Count());
            Assert.AreEqual(1, _commonStateHolder.ActiveDayOffs.Count());
            Assert.AreEqual(1, _commonStateHolder.ScheduleTags.Count());
            Assert.AreEqual(1, _commonStateHolder.ActiveScheduleTags.Count());
            Assert.AreEqual(1, _commonStateHolder.ShiftCategories.Count());
            Assert.AreEqual(1, _commonStateHolder.ActiveShiftCategories.Count());
			Assert.AreEqual(1, _commonStateHolder.WorkflowControlSets.Count);
			Assert.AreEqual(1, _commonStateHolder.MultiplicatorDefinitionSets.Count);
            _mockRep.VerifyAll();
        }

    }
}
