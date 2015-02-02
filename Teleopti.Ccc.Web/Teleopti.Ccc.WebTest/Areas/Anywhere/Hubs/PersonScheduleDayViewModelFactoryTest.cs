using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
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
			var personId = new Guid();
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