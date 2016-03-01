using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	 [TestFixture]
	 public class ScheduleViewModelFactoryTest
	 {
		[Test]
		public void ShoudCreateWeekViewModelByCallingProviderAndMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var weekScheduleDomainDataProvider = MockRepository.GenerateMock<IWeekScheduleDomainDataProvider>();
			var monthScheduleDomainDataProvider = MockRepository.GenerateMock<IMonthScheduleDomainDataProvider>();
			var target = new ScheduleViewModelFactory(mapper, weekScheduleDomainDataProvider, monthScheduleDomainDataProvider);
			var domainData = new WeekScheduleDomainData();
			var viewModel = new WeekScheduleViewModel();

			weekScheduleDomainDataProvider.Stub(x => x.Get(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData)).Return(viewModel);

			var result = target.CreateWeekViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

        [Test]
        public void ShoudCreateMonthViewModelByTwoStepMapping()
        {
            var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var weekScheduleDomainDataProvider = MockRepository.GenerateMock<IWeekScheduleDomainDataProvider>();
			var monthScheduleDomainDataProvider = MockRepository.GenerateMock<IMonthScheduleDomainDataProvider>();
			var target = new ScheduleViewModelFactory(mapper, weekScheduleDomainDataProvider, monthScheduleDomainDataProvider);
			var domainData = new MonthScheduleDomainData();
            var viewModel = new MonthScheduleViewModel();

			monthScheduleDomainDataProvider.Stub(x => x.Get(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<MonthScheduleDomainData, MonthScheduleViewModel>(domainData)).Return(viewModel);

            var result = target.CreateMonthViewModel(DateOnly.Today);

            result.Should().Be.SameInstanceAs(viewModel);
        }
	 }
}