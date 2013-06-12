using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class PersonScheduleViewModelFactoryTest
	{

		private static IPerson FakePerson()
		{
			var person = PersonFactory.CreatePersonWithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
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
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonDeserializer<ExpandoObject>());
			
			target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Date == DateTime.Today)));
		}

		[Test]
		public void ShouldRetrievePersonToMapping()
		{
			var person = FakePerson();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonDeserializer<ExpandoObject>());

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Person == person)));
		}
		
		[Test]
		public void ShouldRetrieveAbsencesToMapping()
		{
			var absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), absenceRepository, personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonDeserializer<ExpandoObject>());
			var absences = new[] { new Absence() };
			absenceRepository.Stub(x => x.LoadAllSortByName()).Return(absences);

			target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Absences == absences)));
		}

		[Test]
		public void ShouldParsePersonScheduleDayReadModelShiftsToMapping()
		{
			var person = FakePerson();
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(person), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonDeserializer<ExpandoObject>());
			var shifts = new {FirstName = "Pierre"};
			var readModel = new PersonScheduleDayReadModel {Shift = Newtonsoft.Json.JsonConvert.SerializeObject(shifts)};

			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(DateOnly.Today, person.Id.Value)).Return(readModel);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => MatchDynamic(s))));
		}

		private static bool MatchDynamic(PersonScheduleData s)
		{
			return s.Shift is ExpandoObject && s.Shift.FirstName == "Pierre";
		}

		[Test]
		public void ShouldCreateViewModelUsingMapping()
		{
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(FakePersonRepository(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>(), new NewtonsoftJsonDeserializer<ExpandoObject>());
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
			var target = new PersonScheduleViewModelFactory(personRepository, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, personAbsenceRepository, new NewtonsoftJsonDeserializer<ExpandoObject>());
			
			var person = PersonFactory.CreatePersonWithGuid("", "");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			var date = new DateTime(2013,5,7);
			var startTime = TimeZoneInfo.ConvertTimeToUtc(date, person.PermissionInformation.DefaultTimeZone());
			var period = new DateTimePeriod(startTime, startTime.AddHours(24));

			var personAbsences = new Collection<IPersonAbsence> { new PersonAbsence(new Scenario(" ")) };
			personAbsenceRepository.Stub(x => x.Find(new[] {person}, period)).Return(personAbsences);

			target.CreateViewModel(person.Id.Value, date);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.PersonAbsences.Equals(personAbsences))));	
		}
	}
}