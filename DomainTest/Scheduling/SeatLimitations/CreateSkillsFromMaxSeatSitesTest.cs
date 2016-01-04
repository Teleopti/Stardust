using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
	[TestFixture]
	public class CreateSkillsFromMaxSeatSitesTest
	{
		[Test]
		public void ShouldCreateSkillForSiteWithMaxSeat()
		{
            var schedulingResultStateHolder = new FakeSchedulingResultStateHolder();
            var target = new CreateSkillsFromMaxSeatSites(schedulingResultStateHolder);
			var site1 = SiteFactory.CreateSimpleSite("site1");
			site1.MaxSeats = 20;
			var site2 = SiteFactory.CreateSimpleSite("site2");

			IList<ISite> sites = new List<ISite> {site1, site2};

			target.CreateSkillList(sites);
			var skills = schedulingResultStateHolder.Skills;
			
			Assert.AreEqual(1, skills.Length);
            Assert.AreEqual(skills[0], site1.MaxSeatSkill);
            Assert.AreEqual(15, skills[0].DefaultResolution);
            Assert.AreEqual(site1.Description.Name, skills[0].Name);
            Assert.AreEqual(ForecastSource.MaxSeatSkill, skills[0].SkillType.ForecastSource);
            Assert.AreEqual(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone.Id, skills[0].TimeZone.Id);
            Assert.AreEqual(96, skills[0].GetTemplateAt(0).TemplateSkillDataPeriodCollection.Count);
            Assert.AreEqual(20, skills[0].GetTemplateAt(0).TemplateSkillDataPeriodCollection[0].MaxSeats);
		}
	}
}