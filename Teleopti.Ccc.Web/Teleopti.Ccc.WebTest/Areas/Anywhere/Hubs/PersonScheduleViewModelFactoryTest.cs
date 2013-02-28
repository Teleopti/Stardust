using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class PersonScheduleViewModelFactoryTest
	{

		[Test]
		public void ShouldPassDateToMapping()
		{
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(MockRepository.GenerateMock<IPersonRepository>(), personScheduleViewModelMapper);
			
			target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Date == DateTime.Today)));
		}

		[Test]
		public void ShouldRetrievePersonAndPassToMapping()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(personRepository, personScheduleViewModelMapper);
			var person = PersonFactory.CreatePersonWithGuid("", "");
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			target.CreateViewModel(person.Id.Value, DateTime.Today);

			personScheduleViewModelMapper.AssertWasCalled(x => x.Map(Arg<PersonScheduleData>.Matches(s => s.Person == person)));
		}

		[Test]
		public void ShouldCreateViewModelUsingMapping()
		{
			var personScheduleViewModelMapper = MockRepository.GenerateMock<IPersonScheduleViewModelMapper>();
			var target = new PersonScheduleViewModelFactory(MockRepository.GenerateMock<IPersonRepository>(), personScheduleViewModelMapper);
			var viewModel = new PersonScheduleViewModel();
			personScheduleViewModelMapper.Stub(x => x.Map(Arg<PersonScheduleData>.Is.Anything)).Return(viewModel);

			var result = target.CreateViewModel(Guid.NewGuid(), DateTime.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

	}
}