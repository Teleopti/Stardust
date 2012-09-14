﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Asm.ViewModelFactory
{
	[TestFixture]
	public class AsmViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateViewModelForGivenPeriod()
		{
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var mapper = MockRepository.GenerateMock<IAsmViewModelMapper>();
			var asmZero = new DateTime(2000, 1, 1, 19, 20, 21);
			var scheduleDays = new List<IScheduleDay>();
			var viewModel = new AsmViewModel();
			var expectedPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			scheduleProvider.Expect(s => s.GetScheduleForPeriod(expectedPeriod)).Return(scheduleDays);
			mapper.Expect(m => m.Map(asmZero, scheduleDays)).Return(viewModel);

			var target = new AsmViewModelFactory(scheduleProvider, mapper);
			var result = target.CreateViewModel(asmZero);

			result.Should().Be.SameInstanceAs(viewModel);
		}
	}

}