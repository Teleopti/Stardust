using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class UniqueSchedulePartExtractorTest
    {
        private UniqueSchedulePartExtractor _target;
        private MockRepository _mocks;
        private List<IScheduleDay> _schedulePartList;
        private IPerson _person1;
        private IPerson _person2;

        [SetUp]
        public void Setup()
        {
            _schedulePartList = new List<IScheduleDay>();
            _mocks = new MockRepository();
            _target = new UniqueSchedulePartExtractor();
            setupPersons();   
        }

        [Test]
        public void VerifyUniqueList()
        {
            createSchedulePartList();

            using (_mocks.Record())
            {
                mockExpectations();
            }

            Assert.AreEqual(4, new List<ISchedulePartExtractor>(_target.ExtractUniqueScheduleParts(_schedulePartList)).Count);
        }

        [Test]
        public void VerifyExtractorWithNullSchedulePeriodDoesNotMakeItToTheList()
        {
            setupPersons2();
            createSchedulePartList();
            using (_mocks.Record())
            {
                mockExpectations();
            }

            Assert.AreEqual(2, new List<ISchedulePartExtractor>(_target.ExtractUniqueScheduleParts(_schedulePartList)).Count);
        }

        private void createSchedulePartList()
        {
            for (var i = 0; i < 10; i++)
            {
                _schedulePartList.Add(_mocks.StrictMock<IScheduleDay>());
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

            IPersonContract personContract = PersonContractFactory.CreateFulltimePersonContractWithWorkingWeekContractSchedule();
            _person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 5, 1), personContract, TeamFactory.CreateSimpleTeam()));
            _person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 5, 1), personContract, TeamFactory.CreateSimpleTeam()));
        }

        private void setupPersons2()
        {
            _person1 = PersonFactory.CreatePerson();
            _person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            ISchedulePeriod schedulePeriod = new SchedulePeriod(new DateOnly(2010, 1, 4), SchedulePeriodType.Day, 7);
            _person1.AddSchedulePeriod(schedulePeriod);

            _person2 = PersonFactory.CreatePerson();
            _person2.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            schedulePeriod = new SchedulePeriod(new DateOnly(2009, 12, 1), SchedulePeriodType.Month, 1);
            _person2.AddSchedulePeriod(schedulePeriod);
            IPersonContract personContract = PersonContractFactory.CreateFulltimePersonContractWithWorkingWeekContractSchedule();
            _person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 5, 1), personContract, TeamFactory.CreateSimpleTeam()));
            _person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 5, 1), personContract, TeamFactory.CreateSimpleTeam()));
        }

        private void mockExpectations()
        {
            DateTimePeriod dt = new DateTimePeriod(2010,1,1,2010,1,2);
            for (int i = 0; i < 5; i++)
            {
                Expect.Call(_schedulePartList[i].Person).Return(_person1).Repeat.Any();
                Expect.Call(_schedulePartList[i].DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(dt.StartDateTime.AddDays(i)), TimeZoneInfo.Utc)).Repeat.Any();
            }
            for (int i = 0; i < 5; i++)
            {
                Expect.Call(_schedulePartList[i + 5].Person).Return(_person2).Repeat.Any();
				Expect.Call(_schedulePartList[i + 5].DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(dt.StartDateTime.AddDays(i)), TimeZoneInfo.Utc)).Repeat.Any();
            }
        }
    }
}
