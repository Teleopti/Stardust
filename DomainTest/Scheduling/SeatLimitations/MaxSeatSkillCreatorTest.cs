using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
	[DomainTest]
	public class MaxSeatSkillCreatorTest
	{
		public MaxSeatSkillCreator Target;

		[Test]
		public void ShouldCreate()
		{
			var personOnSite = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2016, 3, 3));
			var site = personOnSite.Period(new DateOnly(2016, 3, 3)).Team.Site;
			site.MaxSeats = 100;
			var skillOn15 = SkillFactory.CreateSkill("skill15",
				new SkillTypePhone(new Description("Phone"), ForecastSource.InboundTelephony), 15);
			var result = Target.CreateMaxSeatSkills(new DateOnlyPeriod(2016, 3, 3, 2016, 3, 3), new Scenario("hej"),
				new List<IPerson> { personOnSite }, new List<ISkill> { skillOn15 });

			var skillToAdd = result.SkillsToAddToStateholder.First();
			skillToAdd.Name.Should().Be.EqualTo(site.Description.Name);
			site.MaxSeatSkill.Should().Be.EqualTo(skillToAdd);

			var skillDayToAdd = result.SkillDaysToAddToStateholder.First();
			skillDayToAdd.Key.Should().Be.EqualTo(skillToAdd);
			skillDayToAdd.Value.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldWorkAndCreate15MinuteIntervalIfSomeSkillsAreOn15Minute()
		{
			var personOnSite = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2016, 3, 3));
			var site = personOnSite.Period(new DateOnly(2016, 3, 3)).Team.Site;
			site.MaxSeats = 100;
			var skillOn15 = SkillFactory.CreateSkill("skill15",
				new SkillTypePhone(new Description("Phone"), ForecastSource.InboundTelephony), 15);
			var skillOn30 = SkillFactory.CreateSkill("skill30",
				new SkillTypePhone(new Description("Phone"), ForecastSource.InboundTelephony), 30);
			var result = Target.CreateMaxSeatSkills(new DateOnlyPeriod(2016, 3, 3, 2016, 3, 3), new Scenario("hej"),
				new List<IPerson> {personOnSite}, new List<ISkill> {skillOn15, skillOn30});
			var skillToAdd = result.SkillsToAddToStateholder.First();
			skillToAdd.DefaultResolution.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldWorkAndCreate30MinuteIntervalIfAllSkillsAreOn30Minute()
		{
			var personOnSite = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2016, 3, 3));
			var site = personOnSite.Period(new DateOnly(2016, 3, 3)).Team.Site;
			site.MaxSeats = 100;
			var skillOn30 = SkillFactory.CreateSkill("skill30",
				new SkillTypePhone(new Description("Phone"), ForecastSource.InboundTelephony), 30);
			var result = Target.CreateMaxSeatSkills(new DateOnlyPeriod(2016, 3, 3, 2016, 3, 3), new Scenario("hej"),
				new List<IPerson> { personOnSite }, new List<ISkill> { skillOn30 });
			var skillToAdd = result.SkillsToAddToStateholder.First();
			skillToAdd.DefaultResolution.Should().Be.EqualTo(30);
		}

		[Test]
		public void ShouldHandleEmptySkillList()
		{
			var personOnSite = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2016, 3, 3));
			var site = personOnSite.Period(new DateOnly(2016, 3, 3)).Team.Site;
			site.MaxSeats = 100;
			var result = Target.CreateMaxSeatSkills(new DateOnlyPeriod(2016, 3, 3, 2016, 3, 3), new Scenario("hej"),
				new List<IPerson> { personOnSite }, new List<ISkill> ());
			var skillToAdd = result.SkillsToAddToStateholder.First();
			skillToAdd.DefaultResolution.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldHandleEmptyPersonList()
		{
			var personOnSite = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2016, 3, 3));
			var site = personOnSite.Period(new DateOnly(2016, 3, 3)).Team.Site;
			site.MaxSeats = 100;
			var result = Target.CreateMaxSeatSkills(new DateOnlyPeriod(2016, 3, 3, 2016, 3, 3), new Scenario("hej"),
				new List<IPerson> (), new List<ISkill>());
			result.SkillsToAddToStateholder.Should().Be.Empty();
		}
	}
}