using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
	[TestFixture]
	public class CreatePersonalSkillsFromMaxSeatSitesTest
	{
		private ICreatePersonalSkillsFromMaxSeatSites _target;
		private ISite _siteWithMax;
		private ISite _site;
		private IPerson _person1;
		private IPerson _person2;

		[SetUp]
		public void Setup()
		{
			_target = new CreatePersonalSkillsFromMaxSeatSites();
			_site = SiteFactory.CreateSiteWithOneTeam("Team1");
			_siteWithMax = SiteFactory.CreateSiteWithOneTeam("Team2");
			_siteWithMax.MaxSeatSkill = SkillFactory.CreateSiteSkill("Tjoff");
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
		}

		[Test]
		public void ShouldAddSkillOnPersonPeriodWhenSiteIsMaxSeatSite()
		{
			_person1.Period(new DateOnly()).Team = _site.TeamCollection[0];
			_person2.Period(new DateOnly()).Team = _siteWithMax.TeamCollection[0];

			var period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 1, 1));
			IList<IPerson> persons = new List<IPerson>{ _person1, _person2};

			_target.Process(period, persons);

			Assert.AreEqual(0, _person1.Period(new DateOnly()).PersonMaxSeatSkillCollection.Count);
			Assert.AreEqual(1, _person2.Period(new DateOnly()).PersonMaxSeatSkillCollection.Count);
			Assert.AreEqual(_siteWithMax.MaxSeatSkill, _person2.Period(new DateOnly()).PersonMaxSeatSkillCollection[0].Skill);
			Assert.AreEqual(new Percent(1), _person2.Period(new DateOnly()).PersonMaxSeatSkillCollection[0].SkillPercentage);
		}

		[Test]
		public void ShouldIgnorePersonWithNoPeriod()
		{
			_person1.RemoveAllPersonPeriods();

			var period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 1, 1));
			IList<IPerson> persons = new List<IPerson> { _person1 };

			_target.Process(period, persons);

			Assert.IsNull(_person1.Period(new DateOnly(2010, 1, 1)));
		}

		[Test]
		public void ShouldHandleMultiplePeriodsOnPerson()
		{
			_person2.Period(new DateOnly()).Team = _siteWithMax.TeamCollection[0];
			IPersonContract contract = PersonContractFactory.CreatePersonContract();
			_person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2010, 1, 2), contract, _site.TeamCollection[0]));
			_person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2010, 1, 3), contract, _siteWithMax.TeamCollection[0]));

			var period = new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 1, 10));
			IList<IPerson> persons = new List<IPerson> { _person2 };

			_target.Process(period, persons);

			Assert.AreEqual(1, _person2.Period(new DateOnly(2010, 1, 1)).PersonMaxSeatSkillCollection.Count);
			Assert.AreEqual(0, _person2.Period(new DateOnly(2010, 1, 2)).PersonMaxSeatSkillCollection.Count);
			Assert.AreEqual(1, _person2.Period(new DateOnly(2010, 1, 3)).PersonMaxSeatSkillCollection.Count);
		}

		
	}
}