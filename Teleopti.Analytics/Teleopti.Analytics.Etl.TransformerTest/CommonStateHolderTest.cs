using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class CommonStateHolderTest
    {
        private ICommonStateHolder _target;
        private MockRepository _mocks;
        private IRaptorRepository _raptorRepository;
    	private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
			

            _raptorRepository = _mocks.StrictMock<IRaptorRepository>();
            
        	_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelper(_raptorRepository, null, null, null);
			
            _target = new CommonStateHolder(_jobParameters);
        }

        [Test]
        public void VerifyGetSchedules()
        {
            var period = new DateTimePeriod(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            IScheduleDictionary scheduleDictionary;
            IScenario scenario = new Scenario("blabla");
            using (_mocks.Record())
            {
                IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(new DateTimePeriod());
                Expect.Call(_raptorRepository.LoadSchedule(period, scenario, _target)).Return(new ScheduleDictionary(scenario,
                                                                                                           scheduleDateTimePeriod));
            }

            using (_mocks.Playback())
            {
                scheduleDictionary = _target.GetSchedules(period, scenario);
            }

            Assert.AreEqual(0, scheduleDictionary.Count);
        }

        [Test]
        public void VerifyUserCollection()
        {
            IList<IPerson> userCollection = new List<IPerson>();
            userCollection.Add(new Person());

            using (_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadUser()).Return(userCollection);
            }

            using (_mocks.Playback())
            {
                userCollection = _target.UserCollection;
            }

            Assert.AreEqual(1, userCollection.Count);
        }

        [Test]
        public void VerifyGetSchedulePartPerPersonAndDate()
        {
            IPerson person = new Person();
            ICollection<IPerson> personCollection = new List<IPerson> { person };
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var scheduleDateTimePeriod = _mocks.StrictMock<IScheduleDateTimePeriod>();

            var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 3);

            var dummyScheduleRange = _mocks.StrictMock<IScheduleRange>();
            var part1 = _mocks.StrictMock<IScheduleDay>();
            var part2 = _mocks.StrictMock<IScheduleDay>();
            var part3 = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod);
                Expect.Call(scheduleDateTimePeriod.VisiblePeriod).Return(period);
                Expect.Call(scheduleDictionary.Keys).Return(personCollection);
                Expect.Call(scheduleDictionary[person]).Return(dummyScheduleRange).Repeat.Any();
                Expect.Call(dummyScheduleRange.ScheduledDay(new DateOnly(2000, 1, 1))).Return(part1);
                Expect.Call(dummyScheduleRange.ScheduledDay(new DateOnly(2000, 1, 2))).Return(part2);
                Expect.Call(dummyScheduleRange.ScheduledDay(new DateOnly(2000, 1, 3))).Return(part3);
            }

            using (_mocks.Playback())
            {
                IList<IScheduleDay> scheduleParts = _target.GetSchedulePartPerPersonAndDate(scheduleDictionary);
                Assert.AreEqual(3, scheduleParts.Count);
                Assert.IsTrue(scheduleParts.Contains(part1));
                Assert.IsTrue(scheduleParts.Contains(part2));
                Assert.IsTrue(scheduleParts.Contains(part3));
            }
        }

		[Test]
		public void ShouldLoadUtcAndDefaultTimeZoneAndInitiateTimeZonesUsedByDataSourcesIfNotSet()
		{
			_jobParameters.TimeZonesUsedByDataSources = null;
			TimeZoneInfo timeZoneUsedByDataSource = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
			IList<TimeZoneInfo> timeZones;

			using (_mocks.Record())
			{
				Expect.Call(_raptorRepository.LoadTimeZonesInUse()).Return(new List<TimeZoneInfo>());
				Expect.Call(_raptorRepository.LoadTimeZonesInUseByDataSource()).Return(new List<TimeZoneInfo> { timeZoneUsedByDataSource });
			}

			using (_mocks.Playback())
			{
				timeZones = new List<TimeZoneInfo>(_target.TimeZoneCollection);
			}

			Assert.AreEqual(3, timeZones.Count);
			Assert.IsTrue(timeZones.Contains(TimeZoneInfo.FindSystemTimeZoneById("UTC")));
			Assert.IsTrue(timeZones.Contains(_jobParameters.DefaultTimeZone));
			Assert.IsTrue(timeZones.Contains(timeZoneUsedByDataSource));
		}

        [Test]
		public void ShouldLoadTimeZoneCollectionWithTimeZonesUsedByDataSourceAndClient()
        {			
			TimeZoneInfo timeZoneUsedByDataSource = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
			_jobParameters.TimeZonesUsedByDataSources = new List<TimeZoneInfo> {timeZoneUsedByDataSource};

        	TimeZoneInfo usedByClient = TimeZoneInfo.FindSystemTimeZoneById("Greenwich Standard Time");
			IList<TimeZoneInfo> timeZonesInUseByClient = new List<TimeZoneInfo> {usedByClient};
        	IList<TimeZoneInfo> timeZones;

            using (_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadTimeZonesInUse()).Return(timeZonesInUseByClient);
            }

            using (_mocks.Playback())
            {
                timeZones = new List<TimeZoneInfo>(_target.TimeZoneCollection);
            }

            Assert.AreEqual(4, timeZones.Count);
			Assert.IsTrue(timeZones.Contains(timeZoneUsedByDataSource));
			Assert.IsTrue(timeZones.Contains(usedByClient));
        }

        [Test]
        public void VerifySomeCollections()
        {
            List<IAbsence> absences = new List<IAbsence>();
            List<IShiftCategory> shiftCategorysList = new List<IShiftCategory>();
            List<IDayOffTemplate> dayOffTemplatesList = new List<IDayOffTemplate>();
            List<IApplicationFunction> applicationFunctionsList = new List<IApplicationFunction>();
            List<IAvailableData> availableDataList = new List<IAvailableData>();
            List<IApplicationRole> applicationRoleList = new List<IApplicationRole>();

            using (_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadAbsence()).Return(absences).Repeat.Once();
                Expect.Call(_raptorRepository.LoadShiftCategory()).Return(shiftCategorysList).Repeat.Once();
                Expect.Call(_raptorRepository.LoadDayOff()).Return(dayOffTemplatesList).Repeat.Once();
                Expect.Call(_raptorRepository.LoadApplicationFunction()).Return(applicationFunctionsList).Repeat.Once();
                Expect.Call(_raptorRepository.LoadAvailableData()).Return(availableDataList).Repeat.Once();
                Expect.Call(_raptorRepository.LoadApplicationRole(_target)).Return(applicationRoleList).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(absences.Count, _target.AbsenceCollection.Count);
                Assert.AreEqual(shiftCategorysList.Count, _target.ShiftCategoryCollection.Count);
                Assert.AreEqual(dayOffTemplatesList.Count, _target.DayOffTemplateCollection.Count);
                Assert.AreEqual(applicationFunctionsList.Count, _target.ApplicationFunctionCollection.Count);
                Assert.AreEqual(availableDataList.Count, _target.AvailableDataCollection.Count);
                Assert.AreEqual(applicationRoleList.Count, _target.ApplicationRoleCollection.Count);
            }
        }

        [Test]
        public void VerifyScenarioCollectionDeletedExcluded()
        {
            IScenario scenario = new Scenario("sc") {EnableReporting = true};
            IScenario scenarioDeleted = new Scenario("sc deleted") {EnableReporting = true};
            ((Scenario)scenarioDeleted).SetDeleted();
            IList<IScenario> scenarioCollection = new List<IScenario> {scenario, scenarioDeleted};

            using(_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadScenario()).Return(scenarioCollection).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(1, _target.ScenarioCollectionDeletedExcluded.Count);
                Assert.AreEqual("sc", _target.ScenarioCollectionDeletedExcluded[0].Description.Name);
            }
        }

        [Test]
        public void VerifyContractCollection()
        {
            using(_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadContract()).Return(new List<IContract>()).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                Assert.IsNotNull(_target.ContractCollection);
                Assert.AreEqual(0, _target.ContractCollection.Count());
            }
        }

        [Test]
        public void VerifyContractScheduleCollection()
        {
            using (_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadContractSchedule()).Return(new List<IContractSchedule>()).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                Assert.IsNotNull(_target.ContractScheduleCollection);
                Assert.AreEqual(0, _target.ContractScheduleCollection.Count());
            }
        }

        [Test]
        public void VerifyPartTimePercentageCollection()
        {
            using (_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadPartTimePercentage()).Return(new List<IPartTimePercentage>()).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                Assert.IsNotNull(_target.PartTimePercentageCollection);
                Assert.AreEqual(0, _target.PartTimePercentageCollection.Count());
            }
        }

        [Test]
        public void VerifyRuleSetBagCollection()
        {
            using (_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadRuleSetBag()).Return(new List<IRuleSetBag>()).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                Assert.IsNotNull(_target.RuleSetBagCollection);
                Assert.AreEqual(0, _target.RuleSetBagCollection.Count());
            }
        }

        [Test]
        public void VerifyUserDefinedGroupings()
        {
            using (_mocks.Record())
            {
                Expect.Call(_raptorRepository.LoadUserDefinedGroupings()).Return(new List<IGroupPage>()).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                Assert.IsNotNull(_target.UserDefinedGroupings);
                Assert.AreEqual(0, _target.UserDefinedGroupings.Count());
            }
        }

		[Test]
		public void ShouldReturnPersonsWithId()
		{
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var person1 = new Person();
			person1.SetId(id1);
			var person2 = new Person();
			person2.SetId(id2);

			var persons = new List<IPerson> {person1, person2};
			var ids = new List<Guid> {id1};
			Expect.Call(_raptorRepository.LoadPerson(_target)).Return(persons);

			_mocks.ReplayAll();
			var thePersons = _target.PersonsWithIds(ids);
			Assert.That(thePersons.Count,Is.EqualTo(1));
			Assert.That(thePersons.Contains(person1),Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldKnowIfPermissionsMustRun()
		{
			var time = DateTime.Now;
			var lastTime = new LastChangedReadModel {LastTime = time, ThisTime = time};
			_target.SetThisTime(lastTime,"Permissions");
			Assert.That(_target.PermissionsMustRun(),Is.False);
			lastTime = new LastChangedReadModel { LastTime = time, ThisTime = time.AddHours(1) };
			_target.SetThisTime(lastTime, "Permissions");
			Assert.That(_target.PermissionsMustRun(), Is.True);
		}

	    [Test]
	    public void ShouldSaveLastTime()
	    {
			var time = DateTime.Now;
		    var bu = new BusinessUnit("bu");
			var lastTime = new LastChangedReadModel { LastTime = time, ThisTime = time };
			_target.SetThisTime(lastTime, "Schedules");

			Expect.Call(() =>_raptorRepository.UpdateLastChangedDate(bu, "Schedules", time));
			_mocks.ReplayAll();
			_target.UpdateThisTime("Schedules",bu);
	    }
    }
}
