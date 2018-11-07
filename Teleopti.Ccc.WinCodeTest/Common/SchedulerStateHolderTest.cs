using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

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
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private Guid _guid1;
		private Guid _guid2;
		private Guid _guid3;
		private IPersistableScheduleDataPermissionChecker _permissionChecker;
		private IDisposable auth;

		[SetUp]
		public void Setup()
		{
			var period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
			dtp = new ScheduleDateTimePeriod(period);
			scenario = ScenarioFactory.CreateScenarioAggregate("test", true);
			_person1 = PersonFactory.CreatePerson("first", "last");
			_person2 = PersonFactory.CreatePerson("firstName", "lastName");
			_person3 = PersonFactory.CreatePerson("firstName", "lastName");
			_guid1 = Guid.NewGuid();
			_guid2 = Guid.NewGuid();
			_guid3 = Guid.NewGuid();
			_person1.SetId(_guid1);
			_person2.SetId(_guid2);
			_person3.SetId(_guid3);
			selectedPersons = new List<IPerson>{_person1, _person2};
			var schedulingResultStateHolder = SchedulingResultStateHolderFactory.Create(period);
			target = new SchedulerStateHolder(scenario,
											  new DateOnlyPeriodAsDateTimePeriod(
												dtp.VisiblePeriod.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo()),
												TimeZoneInfoFactory.UtcTimeZoneInfo()), selectedPersons, new DisableDeletedFilter(new CurrentUnitOfWork(new FakeCurrentUnitOfWorkFactory(null))), schedulingResultStateHolder, new TimeZoneGuard());
			target.SetRequestedScenario(scenario);
			mocks = new MockRepository();
			_permissionChecker = new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make());
			auth = CurrentAuthorization.ThreadlyUse(new FullPermission());
		}

		[TearDown]
		public void Teardown()
		{
			auth?.Dispose();
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
		public void ShouldBeAbleToSetTimeZoneInfo()
		{
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
			target.TimeZoneInfo = timeZone;
			Assert.AreEqual(timeZone, target.TimeZoneInfo);
		}

		[Test]
		public void CanLoadScheduleAndLoadedPeriodIsSet()
		{
			IScheduleDateTimePeriod period = new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			IScheduleDictionary scheduleDictionary = new ScheduleDictionary(ScenarioFactory.CreateScenarioAggregate(),period, _permissionChecker, CurrentAuthorization.Make());
			var scheduleStorage = mocks.StrictMock<IFindSchedulesForPersons>();
			var personsProvider = Enumerable.Empty<IPerson>();
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false,false);
			using(mocks.Record())
			{
				Expect.Call(scheduleStorage.FindSchedulesForPersons(target.RequestedScenario, personsProvider, scheduleDictionaryLoadOptions, scheduleDictionary.Period.VisiblePeriod, selectedPersons, true))
					.Return(scheduleDictionary);
			}
			using(mocks.Playback())
			{
				target.LoadSchedules(scheduleStorage, personsProvider, scheduleDictionaryLoadOptions, scheduleDictionary.Period.VisiblePeriod);
			}
			Assert.AreSame(scheduleDictionary, target.Schedules);
			Assert.AreEqual(period.LoadedPeriod(), target.Schedules.Period.LoadedPeriod());
		}

		[Test]
		public void VerifyCanLoadSettings()
		{
			CommonNameDescriptionSetting nameDescriptionSetting = new CommonNameDescriptionSetting();

			DefaultSegment defaultSegment = new DefaultSegment();
			defaultSegment.SegmentLength = 42;
			
			IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
			IRepositoryFactory repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
			ISettingDataRepository settingDataRepository = mocks.StrictMock<ISettingDataRepository>();

			Expect.Call(repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork)).Return(settingDataRepository).Repeat.AtLeastOnce();
			Expect.Call(settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting())).IgnoreArguments().Return(nameDescriptionSetting);
			Expect.Call(settingDataRepository.FindValueByKey("DefaultSegment", new DefaultSegment())).IgnoreArguments().Return(defaultSegment).Repeat.Once();


			mocks.ReplayAll();
			IPerson person = PersonFactory.CreatePerson("first", "last");
			target.LoadSettings(unitOfWork,repositoryFactory);
			Assert.AreEqual(nameDescriptionSetting.BuildFor(person), target.CommonAgentName(person));
			Assert.AreEqual(nameDescriptionSetting,target.CommonNameDescription);


			Assert.AreEqual(42, target.DefaultSegmentLength);
			mocks.VerifyAll();
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
		public void CanFilterPersonsOnPerson()
		{
			target.FilterPersons(new List<IPerson>());
		}

		[Test]
		public void ShouldThrowExceptionOnNullScheduleRepository()
		{
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false,false);
			var scheduleDateTimePeriod = mocks.StrictMock<IScheduleDateTimePeriod>();
			Assert.Throws<ArgumentNullException>(() => target.LoadSchedules(null, Enumerable.Empty<IPerson>(), scheduleDictionaryLoadOptions, scheduleDateTimePeriod.VisiblePeriod));
		}

		[Test]
		public void ShouldGetConsiderShortBreaks()
		{
			target.ConsiderShortBreaks = true;
			Assert.IsTrue(target.ConsiderShortBreaks);
		}

		[Test]
		public void ShouldReturnIsFilteredOnAgentsWhenFilterCountNotEqualToAllPermittedCount()
		{
			target.FilterPersons(new List<IPerson>{_person1});
			var result = target.AgentFilter();
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldReturnIsNotFilteredOnAgentsWhenFilterCountEqualToAllPermittedCount()
		{
			target.FilterPersons(new List<IPerson> { _person1, _person2 });
			var result = target.AgentFilter();
			Assert.IsFalse(result);	
		}

		[Test]
		public void ShouldReturnCombinedFilterOnFilteredPersonDictionary()
		{
			target.FilterPersons(new List<IPerson>{_person1});
			target.FilterPersonsOvertimeAvailability(new List<IPerson>{_person1, _person2});
			target.FilterPersonsHourlyAvailability(new List<IPerson> { _person1, _person2, _person3 });
			var filteredPersons = target.FilteredCombinedAgentsDictionary;

			Assert.AreEqual(1, filteredPersons.Count);
			Assert.IsTrue(filteredPersons.ContainsKey(_guid1));

			target.FilterPersons(new List<IPerson> { _person1, _person3 });
			target.FilterPersonsOvertimeAvailability(new List<IPerson> { _person1, _person2, _person3 });
			target.FilterPersonsHourlyAvailability(new List<IPerson> { _person1, _person2, _person3 });
			filteredPersons = target.FilteredCombinedAgentsDictionary;

			Assert.AreEqual(2, filteredPersons.Count);
			Assert.IsTrue(filteredPersons.ContainsKey(_guid1));
			Assert.IsTrue(filteredPersons.ContainsKey(_guid3));
		}
	}
}
