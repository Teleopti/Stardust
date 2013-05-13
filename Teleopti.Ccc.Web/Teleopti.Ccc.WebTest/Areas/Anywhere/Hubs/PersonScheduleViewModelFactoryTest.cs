using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class PersonScheduleViewModelFactoryTest
	{

		[Test]
		public void ShouldPassDateToMapping()
		{
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(MockRepository.GenerateMock<IPersonRepository>(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>());
			
			target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Date == DateTime.Today)));
		}

		[Test]
		public void ShouldRetrievePersonToMapping()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(personRepository, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>());
			var person = PersonFactory.CreatePersonWithGuid("", "");
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Person == person)));
		}
		
		[Test]
		public void ShouldRetrieveAbsencesToMapping()
		{
			var absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(MockRepository.GenerateMock<IPersonRepository>(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), absenceRepository, personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>());
			var absences = new[] { new Absence() };
			absenceRepository.Stub(x => x.LoadAllSortByName()).Return(absences);

			target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Absences == absences)));
		}

		[Test]
		public void ShouldRetrievePersonScheduleDayReadModelToMapping()
		{
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(MockRepository.GenerateMock<IPersonRepository>(), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>());
			var personId = Guid.NewGuid();
			var readModel = new PersonScheduleDayReadModel();
			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(DateOnly.Today, personId)).Return(readModel);

			target.CreateViewModel(personId, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.PersonScheduleDayReadModel == readModel)));
		}

		[Test]
		public void ShouldParsePersonScheduleDayReadModelShiftsToMapping()
		{
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(MockRepository.GenerateMock<IPersonRepository>(), personScheduleDayReadModelRepository, MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>());
			var personId = Guid.NewGuid();
			var shifts = new {FirstName = "Pierre"};
			var readModel = new PersonScheduleDayReadModel {Shift = Newtonsoft.Json.JsonConvert.SerializeObject(shifts)};

			personScheduleDayReadModelRepository.Stub(x => x.ForPerson(DateOnly.Today, personId)).Return(readModel);

			target.CreateViewModel(personId, DateTime.Today);

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
			var target = new PersonScheduleViewModelFactory(MockRepository.GenerateMock<IPersonRepository>(), MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, MockRepository.GenerateMock<IPersonAbsenceRepository>());
			var viewModel = new PersonScheduleViewModel();
			personScheduleViewModelMapper.Stub(x => x.Map(Arg<PersonScheduleData>.Is.Anything)).Return(viewModel);

			var result = target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldRetrievePersonAbsencesToMapping()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(personRepository, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IAbsenceRepository>(), personScheduleViewModelMapper, personAbsenceRepository);
			
			var person = PersonFactory.CreatePersonWithGuid("", "");
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			var personAbsences = new Collection<IPersonAbsence>() { new PersonAbsence(MockRepository.GenerateMock<IScenario>()) };

			var startDate = new DateTime(2012, 2, 2);
			var utcStartDate = new DateTime(2012, 2, 2, 0, 0, 0, DateTimeKind.Utc);

			personAbsenceRepository.Stub(
				x => x.Find(new List<IPerson>() {person}, new DateTimePeriod(utcStartDate, utcStartDate.AddHours(24))))
			                       .Return(personAbsences);

			target.CreateViewModel(person.Id.Value, startDate);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.PersonAbsences.Equals(personAbsences))));	
		}
	}
}