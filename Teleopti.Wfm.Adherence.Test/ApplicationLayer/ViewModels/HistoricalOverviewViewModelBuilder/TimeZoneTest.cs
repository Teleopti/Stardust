using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;

namespace Teleopti.Wfm.Adherence.Test.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class TimeZoneTest: IIsolateSystem
	{
		public Adherence.ApplicationLayer.ViewModels.HistoricalOverviewViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;		
		public FakeUserTimeZone TimeZone;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
		}
		
		[Test]
		public void ShouldDisplayDaysInUserTimeZone()
		{
			Now.Is("2018-09-30 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-09-30 14:00");						
			Now.Is("2018-09-30 23:00");
			TimeZone.IsSweden();
			
			var data = Target.Build(null, new[] {teamId}).First();

			data.DisplayDays.Should().Have.SameValuesAs(new[] {"09/24", "09/25", "09/26", "09/27", "09/28", "09/29", "09/30"});
		}
		
		[Test]
		public void ShouldGetAgentDaysSequentiallyInUserTimezone()
		{
			Now.Is("2018-09-30 14:00");
			var teamId = Guid.NewGuid();
			Database
				.WithTeam(teamId)
				.WithAgent()
				.WithHistoricalStateChange("2018-09-30 14:00");
			Now.Is("2018-09-30 23:00");
			TimeZone.IsSweden();

			var data = Target.Build(null, new[] {teamId}).First();

			data.Agents.Single().Days.Select(x => x.Date).Should().Have.SameValuesAs(new[] {"20180924", "20180925", "20180926", "20180927", "20180928", "20180929", "20180930"});
		}
		
//		[Test]
//		public void ShouldBuildAdherencePercentage2()
//		{
//			var teamId = Guid.NewGuid();
//			Database
//				.WithTeam(teamId)	
//				.WithAgent()
//				.WithAssignment("2018-10-01")
//				.WithActivity(null, "phone")
//				.WithAssignedActivity("2018-10-01 22:00", "2018-10-02 02:00")
//				.WithHistoricalStateChange("2018-10-01 22:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.In)
//				.WithHistoricalStateChange("2018-10-02 01:00", Ccc.Domain.InterfaceLegacy.Domain.Adherence.Out)
//				;
//			
//			Now.Is("2018-10-03 22:00");
//
//			var data = Target.Build(null, new[] {teamId}).First();
//
//			data.Agents.Single().Days.Single(d => d.Date == "20181001").Adherence.Should().Be(75);
//			data.Agents.Single().Days.Single(d => d.Date == "20181002").Adherence.Should().Be(null);
//		}		
				
		
	}
}