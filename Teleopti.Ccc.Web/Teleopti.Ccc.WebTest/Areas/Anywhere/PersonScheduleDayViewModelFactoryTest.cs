using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PersonScheduleDayViewModelFactoryTest
	{
		private IPersonScheduleDayViewModelMapper _personScheduleDayViewModelMapper;
		private IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;
		private PersonScheduleDayViewModelFactory _target;

		[SetUp]
		public void Setup()
		{
			_personScheduleDayViewModelMapper = MockRepository.GenerateMock<IPersonScheduleDayViewModelMapper>();
			_personScheduleDayReadModelFinder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			_target = new PersonScheduleDayViewModelFactory(_personScheduleDayViewModelMapper, _personScheduleDayReadModelFinder);
		}

		[Test]
		public void ShouldGetPersonScheduleDayViewModel()
		{
			var personId = Guid.Empty;
			var date = new DateTime(2015,1,30);
			var model = new PersonScheduleDayReadModel();
			var expected = new PersonScheduleDayViewModel();

			_personScheduleDayReadModelFinder.Stub(x => x.ForPerson(new DateOnly(date.Year, date.Month, date.Day), personId))
				.IgnoreArguments().Return(model);
			_personScheduleDayViewModelMapper.Stub(x => x.Map(model)).Return(expected);

			var result = _target.CreateViewModel(personId, date);

			result.Should().Be.SameInstanceAs(expected);
		}
	}
}