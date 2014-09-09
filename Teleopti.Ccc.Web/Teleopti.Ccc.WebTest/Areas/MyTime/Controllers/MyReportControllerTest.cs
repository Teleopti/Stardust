﻿using System;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class MyReportControllerTest
	{
		[Test]
		public void Index_WhenUserHasPermissionForMyReport_ShouldReturnPartialView()
		{
			var viewModelFactory = MockRepository.GenerateMock<IMyReportViewModelFactory>();
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var target = new MyReportController(viewModelFactory, null, toggleManager);
			var model = new DailyMetricsViewModel();
			var date = DateOnly.Today;

			viewModelFactory.Stub(x => x.CreateDailyMetricsViewModel(date)).Return(model);

			var result = target.Overview(date);
				
			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldReturnDetailedAdherence()
		{
			var viewModelFactory = MockRepository.GenerateMock<IMyReportViewModelFactory>();
            var toggleManager = MockRepository.GenerateMock<IToggleManager>();
            var target = new MyReportController(viewModelFactory, null, toggleManager);
			var date = DateOnly.Today;
			var model = new DetailedAdherenceViewModel();
			viewModelFactory.Stub(x => x.CreateDetailedAherenceViewModel(date)).Return(model);

			var result = target.AdherenceDetails(date);

			result.Data.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void AdherenceShouldReturnPartialView()
		{
			var controller = new MyReportController(null, null, null);
			controller.Adherence().Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void IndexShouldReturnPartialView()
		{
			var controller = new MyReportController(null, null, null);
			controller.Index().Should().Be.OfType<ViewResult>();
		}
	}

	
}