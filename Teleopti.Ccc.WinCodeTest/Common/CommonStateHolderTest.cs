﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
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

        private CommonStateHolder _commonStateHolder;


        [SetUp]
        public void Setup()
        {
            _mockRep = new MockRepository();
            _commonStateHolder = new CommonStateHolder();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanLoadInstance()
        {
            _absenceColl.Add(AbsenceFactory.CreateAbsence("Sick"));
            _dayOffColl.Add(DayOffFactory.CreateDayOff(new Description("test2")));
            _activityColl.Add(ActivityFactory.CreateActivity("act2"));
            IActivity deleted = ActivityFactory.CreateActivity("deleted");
            deleted.SetDeleted();
            _activityColl.Add(deleted);
            _shiftCategoryColl.Add(ShiftCategoryFactory.CreateShiftCategory("act2"));

            var unitOfWork = _mockRep.DynamicMock<IUnitOfWork>();
            var repositoryFactory = _mockRep.StrictMock<IRepositoryFactory>();
            var absenceRepMock = _mockRep.StrictMock<IAbsenceRepository>();
            var dayOffRepMock = _mockRep.StrictMock<IDayOffRepository>();
            var activityRepMock = _mockRep.StrictMock<IActivityRepository>();
            var shiftCategoryRepMock = _mockRep.StrictMock<IShiftCategoryRepository>();
            var contractRepMock = _mockRep.StrictMock<IContractRepository>();
            var contractScheduleRepMock = _mockRep.StrictMock<IContractScheduleRepository>();
            var scheduleTagRep = _mockRep.StrictMock<IScheduleTagRepository>();

            Expect.Call(repositoryFactory.CreateAbsenceRepository(unitOfWork)).Return(absenceRepMock);
            Expect.Call(repositoryFactory.CreateDayOffRepository(unitOfWork)).Return(dayOffRepMock);
            Expect.Call(repositoryFactory.CreateActivityRepository(unitOfWork)).Return(activityRepMock);
            Expect.Call(repositoryFactory.CreateShiftCategoryRepository(unitOfWork)).Return(shiftCategoryRepMock);
            Expect.Call(repositoryFactory.CreateContractRepository(unitOfWork)).Return(contractRepMock);
            Expect.Call(repositoryFactory.CreateContractScheduleRepository(unitOfWork)).Return(contractScheduleRepMock);
            Expect.Call(repositoryFactory.CreateScheduleTagRepository(unitOfWork)).Return(scheduleTagRep).Repeat.Once();
            Expect.Call(activityRepMock.LoadAll()).IgnoreArguments().Return(_activityColl);
            Expect.Call(dayOffRepMock.LoadAll()).IgnoreArguments().Return(_dayOffColl);
            Expect.Call(absenceRepMock.LoadAll()).IgnoreArguments().Return(_absenceColl);
            Expect.Call(shiftCategoryRepMock.FindAll()).IgnoreArguments().Return(_shiftCategoryColl);
            Expect.Call(contractRepMock.FindAllContractByDescription()).Return(_contractList);
            Expect.Call(contractScheduleRepMock.LoadAllAggregate()).Return(_contractScheduleColl);
            Expect.Call(scheduleTagRep.LoadAll()).Return(new List<IScheduleTag>()).Repeat.Once();

            _mockRep.ReplayAll();
            _commonStateHolder.LoadCommonStateHolder(repositoryFactory,unitOfWork);

            Assert.AreEqual(1, _commonStateHolder.Absences.Count);
            Assert.AreEqual(2, _commonStateHolder.Activities.Count);
            Assert.AreEqual(1, _commonStateHolder.ActiveActivities.Count);
            Assert.AreEqual(1, _commonStateHolder.DayOffs.Count);
            Assert.AreEqual(1, _commonStateHolder.ShiftCategories.Count);
            _mockRep.VerifyAll();
        }

    }
}
