using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
    [TestFixture]
    [DomainTest]
    public class SendUpdateStaffingReadModelHandlerTest
    {
        public SendUpdateStaffingReadModelHandler Target;
        public FakeBusinessUnitRepository BusinessUnitRepository;
        public FakeEventPublisher Publisher;
        public MutableNow Now;
	    public FakeJobStartTimeRepository JobStartTimeRepository;

		[Test]
		public void ShouldNotRunResourceCalculationIfItsRecentlyExecuted()
		{
			Now.Is("2016-03-01 09:50");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			JobStartTimeRepository.CheckAndUpdate(60, bu.Id.Value);
			Now.Is("2016-03-01 10:00");
			Target.Handle(new TenantMinuteTickEvent());
			Publisher.PublishedEvents.Count().Should().Be.EqualTo(0);
		}

		[Test]
        public void ShouldRunResourceCalculation()
        {
			Now.Is("2016-03-01 10:00");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			Target.Handle(new TenantMinuteTickEvent());
            Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
	        (Publisher.PublishedEvents.First() as UpdateStaffingLevelReadModelEvent).LogOnBusinessUnitId.Should()
		        .Be.EqualTo(bu.Id.Value);
        }

		[Test]
		public void ShouldExecuteJobOnlyFor1Bu()
		{
			Now.Is("2016-03-01 06:10");
			var bu = BusinessUnitFactory.CreateWithId("bu");
			var bu2 = BusinessUnitFactory.CreateWithId("B2");
			BusinessUnitRepository.Add(bu);
			BusinessUnitRepository.Add(bu2);

			JobStartTimeRepository.CheckAndUpdate(60, bu.Id.Value);
			Now.Is("2016-03-01 08:10");
			JobStartTimeRepository.CheckAndUpdate(60, bu2.Id.Value);
			Target.Handle(new TenantMinuteTickEvent());
			Publisher.PublishedEvents.Count().Should().Be.EqualTo(1);
			(Publisher.PublishedEvents.First() as UpdateStaffingLevelReadModelEvent).LogOnBusinessUnitId.Should()
				.Be.EqualTo(bu.Id.Value);
		}
    }
}
