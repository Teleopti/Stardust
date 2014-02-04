using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ScheduleMatrixListCreatorTest
    {
        private ScheduleMatrixListCreator _target;
        private MockRepository _mockRepository; 
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleDictionary _scheduleDictionary;
        private IList<IScheduleDay> _scheduleDays;
        private IPerson _person1;
        private IPerson _person2;

        [SetUp]
        public void Setup()
        {
            _scheduleDays = new List<IScheduleDay>();
            _mockRepository = new MockRepository();
            _schedulingResultStateHolder = _mockRepository.StrictMock<ISchedulingResultStateHolder>();
            _scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
            _target = new ScheduleMatrixListCreator(_schedulingResultStateHolder);
            setupPersons(); 
        }

        [Test]
        public void VerifyCreation()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyUniqueList()
        {
            createSchedulePartList();

            using (_mockRepository.Record())
            {
                mockExpectations();
            }

            Assert.AreEqual(4, _target.CreateMatrixListFromScheduleParts(_scheduleDays).Count);
        }

        private void createSchedulePartList()
        {
            for (var i = 0; i < 10; i++)
            {
                _scheduleDays.Add(_mockRepository.StrictMock<IScheduleDay>());
            }
        }

        private void setupPersons()
        {
            _person1 = PersonFactory.CreatePerson();
            _person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            ISchedulePeriod schedulePeriod = new SchedulePeriod(new DateOnly(2009, 12, 31), SchedulePeriodType.Day, 2);
            _person1.AddSchedulePeriod(schedulePeriod);
            schedulePeriod = new SchedulePeriod(new DateOnly(2010, 1, 4), SchedulePeriodType.Day, 7);
            _person1.AddSchedulePeriod(schedulePeriod);

            _person2 = PersonFactory.CreatePerson();
            _person2.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            schedulePeriod = new SchedulePeriod(new DateOnly(2009, 12, 1), SchedulePeriodType.Month, 1);
            _person2.AddSchedulePeriod(schedulePeriod);

            IPersonContract personContract =
                PersonContractFactory.CreateFulltimePersonContractWithWorkingWeekContractSchedule();
            _person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 5, 1), personContract,
                                                      TeamFactory.CreateSimpleTeam()));
            _person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 5, 1), personContract,
                                                      TeamFactory.CreateSimpleTeam()));
        }

        private void mockExpectations()
        {
            DateTimePeriod dt = new DateTimePeriod(2010,1,1,2010,1,2);

            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.Any();
            Expect.Call(_scheduleDictionary[_person1]).Return(null).Repeat.Any();
            Expect.Call(_scheduleDictionary[_person2]).Return(null).Repeat.Any();
            for (int i = 0; i < 5; i++)
            {
                Expect.Call(_scheduleDays[i].Person).Return(_person1).Repeat.Any();
                Expect.Call(_scheduleDays[i].Period).Return(dt.MovePeriod(TimeSpan.FromDays(i))).Repeat.Any();
            }
            for (int i = 0; i < 5; i++)
            {
                Expect.Call(_scheduleDays[i + 5].Person).Return(_person2).Repeat.Any();
                Expect.Call(_scheduleDays[i + 5].Period).Return(dt.MovePeriod(TimeSpan.FromDays(i))).Repeat.Any();
            }
        }
    }
}
