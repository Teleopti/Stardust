using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.MonthSchedule.Mapping
{
    [TestFixture]
    public class MonthScheduleDomainDataMappingTest
    {
        private IScheduleProvider scheduleProvider;

        [SetUp]
        public void Setup()
        {
            scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
            Mapper.Reset();
            Mapper.Initialize(c => c.AddProfile(new MonthScheduleDomainDataMappingProfile(scheduleProvider)));
        }

        [Test]
        public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }


        [Test]
        public void ShouldMapDate()
        {
            var today = DateOnly.Today;

            var result = Mapper.Map<DateOnly, MonthScheduleDomainData>(today);

            result.CurrentDate.Should().Be.EqualTo(today);
        }


        [Test]
        [SetCulture("sv-SE")]
        public void ShouldMapDays()
        {
            var date = new DateOnly(2014,1,11);
            
            Mapper.Map<DateOnly, MonthScheduleDomainData>(date);

            scheduleProvider.AssertWasCalled(x => x.GetScheduleForPeriod(new DateOnlyPeriod(2013,12,30,2014,2,2)));
        }
    }
}