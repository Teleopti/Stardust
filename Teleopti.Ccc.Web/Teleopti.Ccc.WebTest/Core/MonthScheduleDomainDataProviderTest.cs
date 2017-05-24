using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core
{
    [TestFixture]
    public class MonthScheduleDomainDataProviderTest
    {
        private IScheduleProvider scheduleProvider;
	    private IMonthScheduleDomainDataProvider target;


        [SetUp]
        public void Setup()
        {
            scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			target = new MonthScheduleDomainDataProvider(scheduleProvider, 
				new FakeToggleManager(), MockRepository.GenerateMock<ISeatOccupancyProvider>());
        }

        [Test]
        public void ShouldMapDate()
        {
            var today = DateOnly.Today;
	        scheduleProvider
		        .Stub(sp => sp.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<ScheduleDictionaryLoadOptions>.Is.Anything))
		        .Return(new List<IScheduleDay>());

			var result = target.Get(today);

            result.CurrentDate.Should().Be.EqualTo(today);
        }


        [Test]
        [SetCulture("sv-SE")]
        public void ShouldMapDays()
        {
            var date = new DateOnly(2014,1,11);
			scheduleProvider
				.Stub(sp => sp.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<ScheduleDictionaryLoadOptions>.Is.Anything))
				.Return(new List<IScheduleDay>());

			target.Get(date);

            scheduleProvider.AssertWasCalled(x => x.GetScheduleForPeriod(new DateOnlyPeriod(2013,12,30,2014,2,2)));
        }
    }
}