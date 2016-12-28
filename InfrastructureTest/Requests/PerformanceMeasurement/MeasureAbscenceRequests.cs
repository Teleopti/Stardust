using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.InfrastructureTest.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Requests.PerformanceMeasurement
{
	[TestFixture]
	[InfrastructureTest]
	[Explicit]
	[Toggle(Toggles.AbsenceRequests_Intraday_UseCascading_41969)]
	public class MeasureAbscenceRequests : InfrastructureTestWithOneTimeSetup, ISetup
	{
		public UpdateStaffingLevelReadModel UpdateStaffingLevel;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
		}

		public override void OneTimeSetUp()
		{
			Now.Is("2016-03-16");
			var now = Now.UtcDateTime().Date;
			var period = new DateTimePeriod(now.AddDays(-1), now.AddDays(1));
			UpdateStaffingLevel.Update(period);
		}

		[Test]
		public void DoTheThing()
		{
		}
	}
}