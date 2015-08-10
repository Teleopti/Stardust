﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
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
			var pushMessageProvider = MockRepository.GenerateMock<IPushMessageProvider>();
		    var currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
            var permissionProvider = new FakePermissionProvider();
			var asmZero = new DateTime(2000, 1, 1);
			var scheduleDays = new List<IScheduleDay>();
			var viewModel = new AsmViewModel();
			var expectedPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 3);
		
			pushMessageProvider.Expect(p => p.UnreadMessageCount).Return(2);
			scheduleProvider.Expect(s => s.GetScheduleForPeriod(expectedPeriod)).Return(scheduleDays);
			mapper.Expect(m => m.Map(asmZero, scheduleDays,2)).Return(viewModel);

            var target = new AsmViewModelFactory(scheduleProvider, mapper, pushMessageProvider, permissionProvider, currentUnitOfWorkFactory);
			var result = target.CreateViewModel(asmZero);

			result.Should().Be.SameInstanceAs(viewModel);
		}
	}

}