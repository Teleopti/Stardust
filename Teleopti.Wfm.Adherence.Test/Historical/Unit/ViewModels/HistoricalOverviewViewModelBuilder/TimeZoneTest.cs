using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalOverviewViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class TimeZoneTest: IIsolateSystem
	{
		public Adherence.Historical.HistoricalOverviewViewModelBuilder Target;
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

			data.DisplayDays.Should().Have.SameValuesAs("09/24", "09/25", "09/26", "09/27", "09/28", "09/29", "09/30");
		}
		
		[Test]
		public void ShouldGetAgentDaysInUserTimeZone()
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

			data.Agents.Single().Days.Select(x => x.Date).Should().Have.SameValuesAs("20180924", "20180925", "20180926", "20180927", "20180928", "20180929", "20180930");
		}				
	}
}