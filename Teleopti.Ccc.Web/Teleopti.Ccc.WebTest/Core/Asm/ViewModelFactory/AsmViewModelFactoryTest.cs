using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
			var today = new DateOnly(2001, 1, 1);
			var expectedPeriod = new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1));
			var scheduleDays = new List<IScheduleDay>();
			var viewModel = new AsmViewModel();

			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var nowProvider = MockRepository.GenerateMock<INow>();

			nowProvider.Expect(n => n.Date()).Return(today);
			scheduleProvider.Expect(s => s.GetScheduleForPeriod(expectedPeriod)).Return(scheduleDays);
			mapper.Expect(m => m.Map<IEnumerable<IScheduleDay>, AsmViewModel>(scheduleDays)).Return(viewModel);
			var target = new AsmViewModelFactory(nowProvider,scheduleProvider, mapper);

			var result = target.CreateViewModel();

			result.Should().Be.SameInstanceAs(viewModel);

		}
	}

}