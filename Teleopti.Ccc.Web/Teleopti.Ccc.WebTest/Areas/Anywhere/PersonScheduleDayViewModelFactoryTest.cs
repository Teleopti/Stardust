using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PersonScheduleDayViewModelFactoryTest
	{
		private IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;
		private PersonScheduleDayViewModelFactory _target;

		[SetUp]
		public void Setup()
		{
			_personScheduleDayReadModelFinder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			_target = new PersonScheduleDayViewModelFactory(new FakeUserTimeZone(), _personScheduleDayReadModelFinder);
		}

		[Test]
		public void ShouldGetPersonScheduleDayViewModel()
		{
			var personId = Guid.NewGuid();
			var date = new DateTime(2015,1,30);
			var model = new PersonScheduleDayReadModel {PersonId = personId};

			_personScheduleDayReadModelFinder.Stub(x => x.ForPerson(new DateOnly(date), personId))
				.IgnoreArguments().Return(model);
			
			var result = _target.CreateViewModel(personId, date);

			result.Person.Should().Be.EqualTo(personId);
		}
	}
}