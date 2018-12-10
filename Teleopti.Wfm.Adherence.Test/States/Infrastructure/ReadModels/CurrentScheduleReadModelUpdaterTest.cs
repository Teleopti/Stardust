using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.ReadModels
{
	[TestFixture]
	[DatabaseTest]
	public class CurrentScheduleReadModelUpdaterTest : IIsolateSystem
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

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}