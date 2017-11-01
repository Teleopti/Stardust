using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SiteTeamChangedHandlers
{
	[DomainTest]
	public class AnalyticsTeamUpdaterTest
	{
		public AnalyticsTeamUpdater Target;
		public FakeAnalyticsTeamRepository AnalyticsTeamRepository;
		public FakeAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;

		[Test]
		public void ShouldUpdateTeamName()
		{
			var team = new AnalyticTeam
			{
				TeamCode = Guid.NewGuid(),
				Name = "MyTeam"
			};
			
			AnalyticsTeamRepository.Has(team);
			AnalyticsPersonPeriod personPeriod = new AnalyticsPersonPeriod()
			{
				PersonCode = Guid.NewGuid(),
				TeamCode = team.TeamCode.Value,
				TeamName = team.Name
			} ;
			AnalyticsPersonPeriodRepository.AddPersonPeriod(personPeriod);

			var changeEvent = new TeamNameChangedEvent
			{
				TeamId = team.TeamCode.Value,
				Name = "newTeamName"
			};

			Target.Handle(changeEvent);

			AnalyticsTeamRepository.GetTeams().First().Name
				.Should().Be.EqualTo(changeEvent.Name);
			AnalyticsPersonPeriodRepository.GetPersonPeriods(personPeriod.PersonCode).First().TeamName
				.Should().Be.EqualTo(changeEvent.Name);
		}


	}
}
