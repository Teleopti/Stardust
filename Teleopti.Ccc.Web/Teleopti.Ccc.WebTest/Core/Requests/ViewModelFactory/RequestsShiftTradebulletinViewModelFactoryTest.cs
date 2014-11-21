using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	[TestFixture]
	public class RequestsShiftTradebulletinViewModelFactoryTest
	{
		[Test]
		public void ShouldRetrieveShiftTradeBulletinViewModel()
		{
			var mapper = MockRepository.GenerateMock<IShiftTradeScheduleViewModelMapper>();
			var target = new RequestsShiftTradebulletinViewModelFactory(mapper);
			var viewModel = new ShiftTradeScheduleViewModel();
			var data = new ShiftTradeScheduleViewModelDataForAllTeams { ShiftTradeDate = DateOnly.Today, TeamIds = new List<Guid>(){ Guid.NewGuid()} };

			mapper.Stub(x => x.MapForBulletin(Arg<ShiftTradeScheduleViewModelDataForAllTeams>.Is.Anything)).Return(viewModel);

			var result = target.CreateShiftTradeBulletinViewModel(data);
			result.Should().Be.SameInstanceAs(viewModel);
		}

	}
}
