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
			var target = new RequestsShiftTradeBulletinViewModelFactory(mapper);
			var viewModel = new ShiftTradeScheduleViewModel();
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, TeamIdList = new List<Guid>() { Guid.NewGuid() } };

			mapper.Stub(x => x.MapForBulletin(Arg<ShiftTradeScheduleViewModelData>.Is.Anything)).Return(viewModel);

			var result = target.CreateShiftTradeBulletinViewModel(data);
			result.Should().Be.SameInstanceAs(viewModel);
		}

	}
}
