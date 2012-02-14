﻿using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	 [TestFixture]
	 public class ScheduleViewModelFactoryTest
	 {
		[Test]
		public void ShoudCreateViewModelByTwoStepMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new ScheduleViewModelFactory(mapper);
			var domainData = new WeekScheduleDomainData();
			var viewModel = new WeekScheduleViewModel();

			mapper.Stub(x => x.Map<DateOnly, WeekScheduleDomainData>(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData)).Return(viewModel);

			var result = target.CreateWeekViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}
	 }
}