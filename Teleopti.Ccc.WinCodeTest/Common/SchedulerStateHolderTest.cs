﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class SchedulerStateHolderTest
    {
        private SchedulerStateHolder target;
        private ScheduleDateTimePeriod dtp;
        private MockRepository mocks;
        private IScenario scenario;
        private IList<IPerson> selectedPersons;

        [SetUp]
        public void Setup()
        {
            scenario = ScenarioFactory.CreateScenarioAggregate();
            dtp = new ScheduleDateTimePeriod(new DateTimePeriod(2000,1,1,2001,1,1));
            IPerson person = PersonFactory.CreatePerson("first", "last");
            selectedPersons = new List<IPerson>{person};
        	target = new SchedulerStateHolder(scenario,
        	                                  new DateOnlyPeriodAsDateTimePeriod(
        	                                  	dtp.VisiblePeriod.ToDateOnlyPeriod(CccTimeZoneInfoFactory.UtcTimeZoneInfo()),
        	                                  	CccTimeZoneInfoFactory.UtcTimeZoneInfo()), selectedPersons);
            mocks = new MockRepository();
        }

        [Test]
        public void LoadScenarioCanBeRead()
        {
            Assert.AreEqual(scenario,target.RequestedScenario);
        }

        [Test]
        public void LoadPeriodCanBeRead()
        {
            Assert.AreEqual(dtp.VisiblePeriod, target.RequestedPeriod.Period());
        }

        [Test]
        public void VerifyDefaultValueForLoadedPeriod()
        {
            Assert.IsNull(target.LoadedPeriod);
        }

        [Test]
        public void ShouldBeAbleToSetTimeZoneInfo()
        {
            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
            target.TimeZoneInfo = timeZone;
            Assert.AreEqual(timeZone, target.TimeZoneInfo);
        }

        [Test]
        public void CanLoadScheduleAndLoadedPeriodIsSet()
        {
            IScheduleDateTimePeriod period = new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            IScheduleDictionary scheduleDictionary = new ScheduleDictionary(ScenarioFactory.CreateScenarioAggregate(),period);
            IScheduleRepository scheduleRepository = mocks.StrictMock<IScheduleRepository>();
            IPersonProvider personsProvider = mocks.StrictMock<IPersonProvider>();
            IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = mocks.StrictMock<IScheduleDictionaryLoadOptions>();
            using(mocks.Record())
            {
                Expect.Call(scheduleRepository.FindSchedulesForPersons(scheduleDictionary.Period, target.RequestedScenario, personsProvider, scheduleDictionaryLoadOptions, selectedPersons))
                    .Return(scheduleDictionary);
            }
            using(mocks.Playback())
            {
                target.LoadSchedules(scheduleRepository, personsProvider, scheduleDictionaryLoadOptions, scheduleDictionary.Period);
            }
            Assert.AreSame(scheduleDictionary, target.Schedules);
            Assert.AreEqual(period.LoadedPeriod(), target.LoadedPeriod);
        }

        [Test]
        public void VerifyCanLoadSettings()
        {
            CommonNameDescriptionSetting nameDescriptionSetting = new CommonNameDescriptionSetting();
            CommonNameDescriptionSettingScheduleExport nameDescriptionSettingScheduleExport = new CommonNameDescriptionSettingScheduleExport();

            DefaultSegment defaultSegment = new DefaultSegment();
            defaultSegment.SegmentLength = 42;
            
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            IRepositoryFactory repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            ISettingDataRepository settingDataRepository = mocks.StrictMock<ISettingDataRepository>();

            Expect.Call(repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork)).Return(settingDataRepository).Repeat.AtLeastOnce();
            Expect.Call(settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting())).IgnoreArguments().Return(nameDescriptionSetting);
            Expect.Call(settingDataRepository.FindValueByKey("CommonNameDescriptionScheduleExport", new CommonNameDescriptionSettingScheduleExport())).IgnoreArguments().Return(nameDescriptionSettingScheduleExport);
            Expect.Call(settingDataRepository.FindValueByKey("DefaultSegment", new DefaultSegment())).IgnoreArguments().Return(defaultSegment).Repeat.Once();


            mocks.ReplayAll();
            IPerson person = PersonFactory.CreatePerson("first", "last");
            target.LoadSettings(unitOfWork,repositoryFactory);
            Assert.AreEqual(nameDescriptionSetting.BuildCommonNameDescription(person), target.CommonAgentName(person));
            Assert.AreEqual(nameDescriptionSetting,target.CommonNameDescription);

            Assert.AreEqual(nameDescriptionSettingScheduleExport.BuildCommonNameDescription(person), target.CommonAgentNameScheduleExport(person));
            Assert.AreEqual(nameDescriptionSettingScheduleExport, target.CommonNameDescriptionScheduleExport);

            Assert.AreEqual(42, target.DefaultSegmentLength);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanLoadPersonRequests()
        {
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            IRepositoryFactory repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            IPersonRequestRepository personRequestRepository = mocks.StrictMock<IPersonRequestRepository>();
            IPerson person = PersonFactory.CreatePerson();
            IList<IPerson> personList = new List<IPerson> { person };
            IPersonRequest personRequest = new PersonRequest(person);
            personRequest.Request = new AbsenceRequest(AbsenceFactory.CreateAbsence("abs"), new DateTimePeriod(2001,1,1,2001,1,2));
            IList<IPersonRequest> requestList = new List<IPersonRequest> {personRequest};
            target.SetRequestedScenario(ScenarioFactory.CreateScenarioAggregate("test", true, false));
            using (mocks.Record())
            {
                Expect.Call(repositoryFactory.CreatePersonRequestRepository(unitOfWork)).Return(personRequestRepository);
                Expect.Call(personRequestRepository.FindAllRequestModifiedWithinPeriodOrPending(personList, new DateTimePeriod(2001, 1, 1, 2001, 1, 2))).Return(requestList).IgnoreArguments();
            }
            using (mocks.Playback())
            {
                target.LoadPersonRequests(unitOfWork, repositoryFactory, new PersonRequestAuthorizationCheckerForTest());
            }

            Assert.AreSame(requestList[0].Id, target.PersonRequests[0].Id);
        }

        [Test]
        public void VerifyRequestUpdateFromBroker()
        {
            IPersonRequestRepository personRequestRepository = mocks.StrictMock<IPersonRequestRepository>();
            IPerson person1 = PersonFactory.CreatePerson();

            IPersonRequest personRequest = new PersonRequest(person1);
            personRequest.SetId(Guid.NewGuid());

            IList<IPersonRequest> originalList = new List<IPersonRequest> {personRequest};
            target.SchedulingResultState.PersonsInOrganization = new Collection<IPerson> {person1};
            using (mocks.Record())
            {
                Expect.Call(personRequestRepository.Find(personRequest.Id.Value))
                    .Return(originalList[0]);
            }
            using (mocks.Playback())
            {
                personRequest.Changed = true;
                IPersonRequest updatedRequest = target.RequestUpdateFromBroker(personRequestRepository,personRequest.Id.Value);
                Assert.AreSame(personRequest, updatedRequest);
            }
            Assert.IsFalse(personRequest.Changed);
            Assert.AreEqual(1, target.PersonRequests.Count);
        }

        [Test]
        public void VerifyRequestUpdateFromBrokerIfNotPresent()
        {
            IPersonRequestRepository personRequestRepository = mocks.StrictMock<IPersonRequestRepository>();
            IPerson person1 = PersonFactory.CreatePerson();

            IPersonRequest personRequest = new PersonRequest(person1);
            personRequest.SetId(Guid.NewGuid());
            target.SchedulingResultState.PersonsInOrganization = new Collection<IPerson> { person1 };
            using (mocks.Record())
            {
                Expect.Call(personRequestRepository.Find(personRequest.Id.Value))
                    .Return(null);
            }
            using (mocks.Playback())
            {
                personRequest.Changed = true;
                IPersonRequest updatedRequest = target.RequestUpdateFromBroker(personRequestRepository, personRequest.Id.Value);
                Assert.IsNull(updatedRequest);
            }
            Assert.IsTrue(personRequest.Changed);
            Assert.AreEqual(0, target.PersonRequests.Count);
        }

        [Test]
        public void VerifyRequestDeleteFromBroker()
        {
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            IRepositoryFactory repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            IPersonRequestRepository personRequestRepository = mocks.StrictMock<IPersonRequestRepository>();
            IPerson person1 = mocks.StrictMock<IPerson>();

            IPersonRequest personRequest = new PersonRequest(person1);
            personRequest.SetId(Guid.NewGuid());

            IList<IPersonRequest> originalList = new List<IPersonRequest> { personRequest };
            IList<IPerson> personList = new List<IPerson> { person1 };
            target.SetRequestedScenario(ScenarioFactory.CreateScenarioAggregate("test", true, false));

            using (mocks.Record())
            {
                Expect.Call(repositoryFactory.CreatePersonRequestRepository(unitOfWork)).Return(personRequestRepository);
                Expect.Call(personRequestRepository.FindAllRequestModifiedWithinPeriodOrPending(personList, new DateTimePeriod(2001, 1, 1, 2001, 1, 2))).Return(originalList).IgnoreArguments();
               
            }
            using (mocks.Playback())
            {
                target.LoadPersonRequests(unitOfWork, repositoryFactory, new PersonRequestAuthorizationCheckerForTest());
                IPersonRequest deletedRequest = target.RequestDeleteFromBroker(personRequest.Id.Value);
                Assert.AreSame(personRequest, deletedRequest);
            }
            Assert.AreEqual(0,target.PersonRequests.Count);
        }

        [Test]
        public void VerifyChangedRequests()
        {
            target.PersonRequests.Add(new PersonRequest(PersonFactory.CreatePerson()));
            Assert.IsFalse(target.ChangedRequests());
            
            target.PersonRequests[0].Changed = true;
            Assert.IsTrue(target.ChangedRequests());
        }

        [Test]
        public void VerifyDaysToBeResourceCalculated()
        {
            target.MarkDateToBeRecalculated(new DateOnly(2007,01,01));
            Assert.AreEqual(1, target.DaysToRecalculate.Count());
            Assert.AreEqual(new DateOnly(2007, 01, 01), target.DaysToRecalculate.First());
            target.ClearDaysToRecalculate();
            Assert.AreEqual(0, target.DaysToRecalculate.Count());
        }

        
        [Test]
        public void CanFilterPersons()
        {
            target.FilterPersons(new List<ITeam>());
        }
        [Test]
        public void CanFilterPersonsOnPerson()
        {
            target.FilterPersons(new List<IPerson>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullScheduleRepository()
        {
            var personProvider = mocks.StrictMock<IPersonProvider>();
            var scheduleDictionaryLoadOptions = mocks.StrictMock<IScheduleDictionaryLoadOptions>();
            var scheduleDateTimePeriod = mocks.StrictMock<IScheduleDateTimePeriod>();
            target.LoadSchedules(null, personProvider, scheduleDictionaryLoadOptions, scheduleDateTimePeriod);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullPeriod()
        {
            var personProvider = mocks.StrictMock<IPersonProvider>();
            var scheduleDictionaryLoadOptions = mocks.StrictMock<IScheduleDictionaryLoadOptions>();
            var scheduleRepository = mocks.StrictMock<IScheduleRepository>();
            target.LoadSchedules(scheduleRepository, personProvider, scheduleDictionaryLoadOptions, null);
        }

    }
}
