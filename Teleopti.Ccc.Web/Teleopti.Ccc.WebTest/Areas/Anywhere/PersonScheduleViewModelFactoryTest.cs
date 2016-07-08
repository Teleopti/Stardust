using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PersonScheduleViewModelFactoryTest
	{
		private IPermissionProvider _permissionProvider;
		private CommonAgentNameProvider _commonAgentNameProvider;
		private ICurrentScenario _currentScenario;

		[SetUp]
		public void Setup()
		{
			_permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var globalSettingRepository = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			globalSettingRepository.Stub(x => x.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting()))
				.IgnoreArguments()
				.Return(new CommonNameDescriptionSetting());
			_commonAgentNameProvider = new CommonAgentNameProvider(globalSettingRepository);
			Mapper.Initialize(c => c.AddProfile(new PersonScheduleViewModelMappingProfile(new FakeUserTimeZone(TimeZoneInfo.Local), new Now())));
			_currentScenario = new FakeCurrentScenario();
		}

		private static IPerson FakePerson()
		{
			var person = PersonFactory.CreatePersonWithGuid("", "");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			person.WorkflowControlSet=new WorkflowControlSet
				{
					SchedulePublishedToDate = DateTime.Today.AddDays(1)
				};
			return person;
		}

		private static IPersonRepository FakePersonRepository()
		{
			return FakePersonRepository(FakePerson());
		}

		private static IPersonRepository FakePersonRepository(IPerson person)
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments().Return(person);
			return personRepository;
		}

		[Test]
		public void ShouldPassDateToMapping()
		{
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			
			target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Date == DateTime.Today)));
		}

		[Test]
		public void ShouldRetrievePersonToMapping()
		{
			var person = FakePerson();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Person == person)));
		}

        [Test]
        public void ShouldRetrieveTimeZoneNameToMapping()
        {
            var person = FakePerson();
            var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
            var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);

            target.CreateViewModel(person.Id.Value, DateTime.Today);

            personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Person.PermissionInformation.DefaultTimeZone().DisplayName == person.PermissionInformation.DefaultTimeZone().DisplayName)));
        }

        [Test]
        public void ShouldRetrieveIanaTimeZoneNameFromWindowsToMapping()
        {
            var person = FakePerson();
            var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
            var ianaTimeZoneProvider = MockRepository.GenerateMock<IIanaTimeZoneProvider>();
						var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, ianaTimeZoneProvider, new UtcTimeZone(), _currentScenario);

            ianaTimeZoneProvider.Stub(x => x.WindowsToIana(person.PermissionInformation.DefaultTimeZone().Id)).Return("America/Washington");

            target.CreateViewModel(person.Id.Value, DateTime.Today);

            personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.IanaTimeZoneOther == "America/Washington")));
        }

		[Test]
		public void ShouldRetrieveMyIanaTimeZoneNameFromWindowsToMapping()
		{
			var person = FakePerson();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var ianaTimeZoneProvider = MockRepository.GenerateMock<IIanaTimeZoneProvider>();
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), 
				MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, 
				MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, 
				ianaTimeZoneProvider, userTimeZone, _currentScenario);

			var brazilTimeZone = TimeZoneInfoFactory.BrazilTimeZoneInfo();
			
			userTimeZone.Stub(x => x.TimeZone()).Return(brazilTimeZone);
			ianaTimeZoneProvider.Stub(x => x.WindowsToIana(brazilTimeZone.Id)).Return("Brazil/Brasilia");

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.IanaTimeZoneLoggedOnUser == "Brazil/Brasilia")));
		}

		[Test]
		public void ShouldRetrieveHasViewConfidentialPermissionToMapping()
		{
			var person = FakePerson();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			_permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
			                                                    DateOnly.Today, person)).Return(true);
			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.HasViewConfidentialPermission)));
		}
		
		[Test]
		public void ShouldRetrieveAbsencesToMapping()
		{
			var absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), absenceRepository, MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			var absences = new[] { new Absence() };
			absenceRepository.Stub(x => x.LoadAllSortByName()).Return(absences);

			target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Absences == absences)));
		}

		[Test]
		public void ShouldRetrieveActivitiesToMapping()
		{
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(),
			                                                MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(),
			                                                MockRepository.GenerateMock<IAbsenceRepository>(),
															activityRepository,
			                                                personScheduleViewModelMapper,
			                                                MockRepository.GenerateMock<IPersonAbsenceRepository>(),
																											new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			var activities = new[] { new Activity("test activity") };
			activityRepository.Stub(x => x.LoadAllSortByName()).Return(activities);
			target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Activities == activities)));
		}

		[Test]
		public void ShouldParsePersonScheduleDayReadModelShiftsToMapping()
		{
			var person = FakePerson();
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			var readModel = new PersonScheduleDayReadModel {Model = Newtonsoft.Json.JsonConvert.SerializeObject(new Model {FirstName = "Pierre"})};

			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(DateOnly.Today, person.Id.Value)).Return(readModel);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Model.FirstName == "Pierre")));
		}

		[Test]
		public void ShouldNotMapScheduleWhenUnpublishedAndNoPermission()
		{
			var person = FakePerson();
			person.WorkflowControlSet=new WorkflowControlSet
				{
					SchedulePublishedToDate = DateTime.Today.AddDays(-1)
				};
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, personAbsenceRepository, new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			var readModel = new PersonScheduleDayReadModel {Model = Newtonsoft.Json.JsonConvert.SerializeObject(new Model {FirstName = "Pierre"})};
			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(DateOnly.Today, person.Id.Value)).Return(readModel);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasNotCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Model != null)));
			personAbsenceRepository.AssertWasNotCalled(x => x.Find(new[] { person }, new DateTimePeriod()), y => y.IgnoreArguments());
		}

		[Test]
		public void ShouldMapScheduleWhenHasViewUnpublishedSchedulePermission()
		{
			_permissionProvider.Stub(
				x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
			                   .Return(true);
			var person = FakePerson();
			person.WorkflowControlSet = new WorkflowControlSet
			{
				SchedulePublishedToDate = DateTime.Today.AddDays(-1)
			};
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, personAbsenceRepository, new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			var readModel = new PersonScheduleDayReadModel {Model = Newtonsoft.Json.JsonConvert.SerializeObject(new Model {FirstName = "Pierre"})};

			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(DateOnly.Today, person.Id.Value)).Return(readModel);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Model.FirstName == "Pierre")));
			personAbsenceRepository.AssertWasCalled(x => x.Find(new[] {person}, new DateTimePeriod(), _currentScenario.Current()), y => y.IgnoreArguments());
		}

		[Test]
		public void ShouldCreateViewModelUsingMapping()
		{
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			var viewModel = new PersonScheduleViewModel();
			personScheduleViewModelMapper.Stub(x => x.Map(Arg<PersonScheduleData>.Is.Anything)).Return(viewModel);

			var result = target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldRetrievePersonAbsencesForPersonsTimeZoneToMapping()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(personRepository, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, personAbsenceRepository, new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);

			var person = FakePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			var date = new DateTime(2013,5,7);
			var startTime = TimeZoneInfo.ConvertTimeToUtc(date, person.PermissionInformation.DefaultTimeZone());
			var period = new DateTimePeriod(startTime, startTime.AddHours(24));

			var personAbsences = new Collection<IPersonAbsence> { new PersonAbsence(new Scenario("s")) };
			personAbsenceRepository.Stub(x => x.Find(new[] {person}, period, _currentScenario.Current())).Return(personAbsences);

			target.CreateViewModel(person.Id.Value, date);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.PersonAbsences.Equals(personAbsences))));	
		}

		[Test]
		public void ShouldRetrievePersonAbsencesInShiftPeriod()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var target = new PersonScheduleViewModelFactory(personRepository, personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, personAbsenceRepository, new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			var person = FakePerson();
			var date = new DateTime(2013, 5, 7);
			var shiftStart = DateTime.SpecifyKind(new DateTime(2013, 5, 7, 19, 0, 0, 0), DateTimeKind.Utc);
			var shiftEnd = DateTime.SpecifyKind(new DateTime(2013, 5, 8, 5, 0, 0, 0), DateTimeKind.Utc);
			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(new DateOnly(date), person.Id.Value))
			                                    .Return(new PersonScheduleDayReadModel
				                                    {
					                                    Start = shiftStart,
					                                    End = shiftEnd
				                                    });
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			var period = new DateTimePeriod(shiftStart, shiftEnd);

			var personAbsences = new Collection<IPersonAbsence> { new PersonAbsence(_currentScenario.Current()) };
			personAbsenceRepository.Stub(x => x.Find(new[] { person }, period, _currentScenario.Current())).Return(personAbsences);

			target.CreateViewModel(person.Id.Value, date);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.PersonAbsences.Equals(personAbsences))));
		}

		[Test]
		public void ShouldRetrievePersonAbsencesExcludeNightShiftFromPreviousDay()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var target = new PersonScheduleViewModelFactory(personRepository, personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, personAbsenceRepository, new NewtonsoftJsonSerializer(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			var person = FakePerson();
			var date = new DateTime(2013, 5, 7);
			var shiftStart = DateTime.SpecifyKind(new DateTime(2013, 5, 6, 19, 0, 0, 0), DateTimeKind.Utc);
			var shiftEnd = DateTime.SpecifyKind(new DateTime(2013, 5, 7, 5, 0, 0, 0), DateTimeKind.Utc);
			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(new DateOnly(date).AddDays(-1), person.Id.Value))
												.Return(new PersonScheduleDayReadModel
												{
													Start = shiftStart,
													End = shiftEnd
												});
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			var endTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2013, 5, 8, 0, 0, 0, 0), person.PermissionInformation.DefaultTimeZone());
			var period = new DateTimePeriod(shiftEnd, endTime);

			var personAbsences = new Collection<IPersonAbsence> { new PersonAbsence(_currentScenario.Current()) };
			personAbsenceRepository.Stub(x => x.Find(new[] { person }, period, _currentScenario.Current())).Return(personAbsences);

			target.CreateViewModel(person.Id.Value, date);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.PersonAbsences.Equals(personAbsences))));
		}

		[Test]
		public void ShouldNotRetrievePersonAbsencesInNonDefaultScenario()
		{
			var personRepository = new FakePersonRepository();
			var personAbsenceRepository = new FakePersonAbsenceRepository();
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper();
			var personScheduleDayReadModelRepository =
				new FakePersonScheduleDayReadModelFinder(new FakePersonAssignmentRepository(), personRepository);
			var absenceRepository = new FakeAbsenceRepository();
			var activityRepository = new FakeActivityRepository();
			var permissionProvider = new FakePermissionProvider();
			var timeZonProvider = new FakeIanaTimeZoneProvider();
			var commonAgentNameProvider = new FakeCommonAgentNameProvider();

			var agent = PersonFactory.CreatePersonWithId();
			personRepository.Add(agent);

			var period1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 15, 0, DateTimeKind.Utc),
				new DateTime(2007, 1, 1, 17, 15, 0, DateTimeKind.Utc));
			var period2 = period1.MovePeriod(TimeSpan.FromHours(1));

			var absence1 = AbsenceFactory.CreateAbsence("ValidAbsence");
			var absence2 = AbsenceFactory.CreateAbsence("InvalidAbsence");

			var layer1 = new AbsenceLayer(absence1, period1);
			var layer2 = new AbsenceLayer(absence2, period2);

			var defaultScenario = _currentScenario.Current();
			var nonDefaultScenario = new Scenario("s");

			var personAbsence1 = new PersonAbsence(agent, defaultScenario, layer1);
			var personAbsence2 = new PersonAbsence(agent, nonDefaultScenario, layer2);

			personAbsenceRepository.Add(personAbsence1);
			personAbsenceRepository.Add(personAbsence2);

			var target = new PersonScheduleViewModelFactory(personRepository, personScheduleDayReadModelRepository
				, absenceRepository, activityRepository,
				personScheduleViewModelMapper, personAbsenceRepository, new NewtonsoftJsonSerializer(), permissionProvider,
				commonAgentNameProvider, timeZonProvider, new UtcTimeZone(), _currentScenario);

			var viewModel = target.CreateViewModel(agent.Id.Value, new DateTime(2007, 1, 1));
			var personAbsences = viewModel.PersonAbsences.ToList();

			personAbsences.Count().Should().Be(1);
			personAbsences.Count(p => p.Name.Equals("ValidAbsence")).Should().Be(1);
			personAbsences.Count(p => p.Name.Equals("InvalidAbsence")).Should().Be(0);
		}
	}
}