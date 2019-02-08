using System;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Areas.Global;


namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PersonScheduleViewModelFactoryTest
	{
		private FakePermissionProvider _permissionProvider;
		private ICommonAgentNameProvider _commonAgentNameProvider;
		private ICurrentScenario _currentScenario;

		[SetUp]
		public void Setup()
		{
			_permissionProvider = new FakePermissionProvider();
			_commonAgentNameProvider = new FakeCommonAgentNameProvider();
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			_currentScenario = new DefaultScenarioFromRepository(scenarioRepository);
		}

		private static IPerson FakePerson()
		{
			var person = PersonFactory.CreatePersonWithGuid("", "");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			person.WorkflowControlSet=new WorkflowControlSet
				{
					SchedulePublishedToDate = DateTime.Today.AddDays(1)
				};
			return person;
		}
		
		private static IPersonRepository FakePersonRepository(IPerson person)
		{
			var personRepository = new FakePersonRepository(null);
			personRepository.Add(person);
			return personRepository;
		}

		[Test]
		public void ShouldPassDateToMapping()
		{
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var person = FakePerson();
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			
			target.CreateViewModel(person.Id.GetValueOrDefault(), DateTime.Today);
			
			personScheduleDayReadModelRepository.AssertWasCalled(x => x.ForPerson(DateOnly.Today, person.Id.Value));
		}
		
        [Test]
        public void ShouldRetrieveIanaTimeZoneNameFromWindowsToMapping()
        {
            var person = FakePerson();
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var ianaTimeZoneProvider = MockRepository.GenerateMock<IIanaTimeZoneProvider>();
						var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, ianaTimeZoneProvider, new UtcTimeZone(), _currentScenario);

            ianaTimeZoneProvider.Stub(x => x.WindowsToIana(person.PermissionInformation.DefaultTimeZone().Id)).Return("America/Washington");

            var result = target.CreateViewModel(person.Id.Value, DateTime.Today);

	        result.IanaTimeZoneOther.Should().Be.EqualTo("America/Washington");
        }

		[Test]
		public void ShouldRetrieveMyIanaTimeZoneNameFromWindowsToMapping()
		{
			var person = FakePerson();
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var ianaTimeZoneProvider = new IanaTimeZoneProvider();
			var userTimeZone = new FakeUserTimeZone(TimeZoneInfoFactory.BrazilTimeZoneInfo());
			var fakePersonRepository = FakePersonRepository(person);
			var target = new PersonScheduleViewModelFactory(fakePersonRepository, new FakePersonScheduleDayReadModelFinder(new FakePersonAssignmentRepository(null), fakePersonRepository, null, null, null), 
				new FakeAbsenceRepository(), new FakeActivityRepository(), personScheduleViewModelMapper, 
				new FakePersonAbsenceRepository(null), NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, 
				ianaTimeZoneProvider, userTimeZone, _currentScenario);
			
			var result = target.CreateViewModel(person.Id.Value, DateTime.Today);

			result.IanaTimeZoneLoggedOnUser.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldRetrieveHasViewConfidentialPermissionToMapping()
		{
			var person = FakePerson();
			var absenceRepository = new FakeAbsenceRepository();
			var absence = AbsenceFactory.CreateAbsence("Sjuk").WithId();
			absence.Confidential = true;
			absenceRepository.Add(absence);
			_permissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ViewConfidential, DateOnly.Today, person);
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), absenceRepository, MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);

			var result = target.CreateViewModel(person.Id.Value, DateTime.Today);

			result.Absences.First().Name.Should().Be.EqualTo("Sjuk");
		}
		
		[Test]
		public void ShouldRetrieveAbsencesToMapping()
		{
			var absenceRepository = new FakeAbsenceRepository();
			var absence = new Absence().WithId();
			absenceRepository.Has(absence);
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var person = FakePerson();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), absenceRepository, MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			
			var result = target.CreateViewModel(person.Id.GetValueOrDefault(), DateTime.Today);
			result.Absences.First().Id.Should().Be.EqualTo(absence.Id.ToString());
		}

		[Test]
		public void ShouldRetrieveActivitiesToMapping()
		{
			var activityRepository = new FakeActivityRepository();
			var activity = new Activity("test activity").WithId();
			activityRepository.Has(activity);

			var person = FakePerson();
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person),
			                                                MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(),
			                                                new FakeAbsenceRepository(), 
															activityRepository,
			                                                personScheduleViewModelMapper,
			                                                MockRepository.GenerateMock<IPersonAbsenceRepository>(),
																											NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);

			var result = target.CreateViewModel(person.Id.GetValueOrDefault(), DateTime.Today);

			result.Activities.First().Id.Should().Be.EqualTo(activity.Id.ToString());
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
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), personScheduleDayReadModelRepository,
				new FakeAbsenceRepository(), new FakeActivityRepository(), personScheduleViewModelMapper, personAbsenceRepository,
				NewtonsoftJsonSerializer.Make(), new FakeNoPermissionProvider(), _commonAgentNameProvider,
				new FakeIanaTimeZoneProvider(), new UtcTimeZone(), _currentScenario);
			var readModel = new PersonScheduleDayReadModel {Model = Newtonsoft.Json.JsonConvert.SerializeObject(new Model {FirstName = "Pierre"})};
			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(DateOnly.Today, person.Id.Value)).Return(readModel);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personAbsenceRepository.AssertWasNotCalled(x => x.Find(new[] { person }, new DateTimePeriod(),_currentScenario.Current()), y => y.IgnoreArguments());
		}

		[Test]
		public void ShouldMapScheduleWhenHasViewUnpublishedSchedulePermission()
		{
			var person = FakePerson();
			person.WorkflowControlSet = new WorkflowControlSet
			{
				SchedulePublishedToDate = DateTime.Today.AddDays(-1)
			};
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), personScheduleDayReadModelRepository, new FakeAbsenceRepository(), new FakeActivityRepository(), personScheduleViewModelMapper, personAbsenceRepository, NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, new FakeIanaTimeZoneProvider(), new UtcTimeZone(), _currentScenario);
			var readModel = new PersonScheduleDayReadModel {Model = Newtonsoft.Json.JsonConvert.SerializeObject(new Model {FirstName = "Pierre"})};

			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(DateOnly.Today, person.Id.Value)).Return(readModel);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personAbsenceRepository.AssertWasCalled(x => x.Find(new[] {person}, new DateTimePeriod(), _currentScenario.Current()), y => y.IgnoreArguments());
		}

		[Test]
		public void ShouldCreateViewModelUsingMapping()
		{
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var person = FakePerson();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
			
			var result = target.CreateViewModel(person.Id.GetValueOrDefault(), DateTime.Today);

			result.Name.Should().Be.EqualTo(_commonAgentNameProvider.CommonAgentNameSettings.BuildFor(person));
		}

		[Test]
		public void ShouldRetrievePersonAbsencesForPersonsTimeZoneToMapping()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var absence = AbsenceFactory.CreateAbsence("test").WithId();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var target = new PersonScheduleViewModelFactory(personRepository, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, personAbsenceRepository, NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);

			var person = FakePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			var date = new DateTime(2013,5,7);
			var startTime = TimeZoneInfo.ConvertTimeToUtc(date, person.PermissionInformation.DefaultTimeZone());
			var period = new DateTimePeriod(startTime, startTime.AddHours(24));

			var personAbsences = new Collection<IPersonAbsence> { new PersonAbsence(person, _currentScenario.Current(),
					new AbsenceLayer(absence, new DateTimePeriod(2013,5,7,9,2013,5,7,11))) };
			personAbsenceRepository.Stub(x => x.Find(new[] {person}, period, _currentScenario.Current())).Return(personAbsences);

			var result = target.CreateViewModel(person.Id.Value, date);

			result.PersonAbsences.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldRetrievePersonAbsencesInShiftPeriod()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var absence = AbsenceFactory.CreateAbsence("test").WithId();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var target = new PersonScheduleViewModelFactory(personRepository, personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, personAbsenceRepository, NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
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

			var personAbsences = new Collection<IPersonAbsence> { new PersonAbsence(person, _currentScenario.Current(),
					new AbsenceLayer(absence, new DateTimePeriod(shiftStart, shiftEnd))) };
			personAbsenceRepository.Stub(x => x.Find(new[] { person }, period, _currentScenario.Current())).Return(personAbsences);

			var result = target.CreateViewModel(person.Id.Value, date);

			result.PersonAbsences.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRetrievePersonAbsencesExcludeNightShiftFromPreviousDay()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var absence = AbsenceFactory.CreateAbsence("test").WithId();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var target = new PersonScheduleViewModelFactory(personRepository, personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), MockRepository.GenerateMock<IActivityRepository>(), personScheduleViewModelMapper, personAbsenceRepository, NewtonsoftJsonSerializer.Make(), _permissionProvider, _commonAgentNameProvider, MockRepository.GenerateMock<IIanaTimeZoneProvider>(), new UtcTimeZone(), _currentScenario);
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

			var personAbsences = new Collection<IPersonAbsence>
			{
				new PersonAbsence(person, _currentScenario.Current(),
					new AbsenceLayer(absence, new DateTimePeriod(shiftStart, shiftEnd)))
			};
			personAbsenceRepository.Stub(x => x.Find(new[] { person }, period, _currentScenario.Current())).Return(personAbsences);

			var result = target.CreateViewModel(person.Id.Value, date);
			result.PersonAbsences.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotRetrievePersonAbsencesInNonDefaultScenario()
		{
			var personRepository = new FakePersonRepository(null);
			var personAbsenceRepository = new FakePersonAbsenceRepository(null);
			var personScheduleViewModelMapper = new PersonScheduleViewModelMapper(new FakeUserTimeZone(TimeZoneInfo.Local), new Now());
			var personScheduleDayReadModelRepository =
				new FakePersonScheduleDayReadModelFinder(new FakePersonAssignmentRepository(null), personRepository, null, null, null);
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
				personScheduleViewModelMapper, personAbsenceRepository, NewtonsoftJsonSerializer.Make(), permissionProvider,
				commonAgentNameProvider, timeZonProvider, new UtcTimeZone(), _currentScenario);

			var viewModel = target.CreateViewModel(agent.Id.Value, new DateTime(2007, 1, 1));
			var personAbsences = viewModel.PersonAbsences.ToList();

			personAbsences.Count().Should().Be(1);
			personAbsences.Count(p => p.Name.Equals("ValidAbsence")).Should().Be(1);
			personAbsences.Count(p => p.Name.Equals("InvalidAbsence")).Should().Be(0);
		}
	}
}