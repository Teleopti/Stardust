using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.Schedules
{
	[TestFixture]
	[DatabaseTest]
	public class CurrentScheduleReadModelUpdaterTest
	{
		public Database Database;
		public ScheduleChangeProcessor Target;
		public IScheduleReader Reader;
		public WithReadModelUnitOfWork WithUnitOfWork;
		public MutableNow Now;

		[Test]
		public void ShouldContainSchedule()
		{
			Database
				.WithAgent()
				.WithActivity("phone")
				.WithAssignment("2017-01-26")
				.WithAssignedActivity("phone", "2017-01-26 08:00", "2017-01-26 17:00")
				;
			Now.Is("2017-01-26 08:00");

			Target.Handle(new TenantMinuteTickEvent());

			WithUnitOfWork.Get(() => Reader.Read())
				.Single().Schedule.Single().Name.Should().Be("phone");
		}
		
	}
}