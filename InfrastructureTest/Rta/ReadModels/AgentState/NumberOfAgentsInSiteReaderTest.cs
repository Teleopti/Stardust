using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[DatabaseTest]
	[TestFixture]
	public class NumberOfAgentsInSiteReaderTest
	{
		public MutableNow Now;
		public Database Database;
		public WithUnitOfWork WithUnitOfWork;
		public INumberOfAgentsInSiteReader Target;
		
		[Test]
		public void ShouldLoadNumberOfAgentesForSite()
		{
			Database
				.WithAgent()
				.WithAgent();
			var siteId = Database.CurrentSiteId();

			var result = WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new[] {siteId}));

			result[siteId].Should().Be(2);
		}

		[Test]
		public void ShouldReturnSiteWithNoAgents()
		{
			Database
				.WithSite();
			var siteId = Database.CurrentSiteId();

			var result = WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new[] { siteId }));

			result[siteId].Should().Be(0);
		}

		[Test]
		public void ShouldNotLoadTerminatedAgent()
		{
			Now.Is("2016-10-17 08:00");
			Database
				.WithAgent()
				.WithTerminatedAgent("2016-10-16");
			var siteId = Database.CurrentSiteId();

			var result = WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new[] { siteId }));

			result[siteId].Should().Be(1);
		}

		[Test]
		public void ShouldOnlyCountAgentOnce()
		{
			Now.Is("2016-11-17 08:00");
			Database
				.WithPerson("ashley")
				.WithPersonPeriod("2016-09-01")
				.WithPersonPeriod("2016-11-01")
				;
			var siteId = Database.CurrentSiteId();

			var result = WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new[] { siteId }));

			result[siteId].Should().Be(1);
		}

		[Test]
		public void ShouldGetForCorrectDate()
		{
			Now.Is("2017-02-21 16:00");
			Database
				.WithPerson()
				.WithPersonPeriod("2017-01-01")
				.WithPersonPeriod("2017-02-22");
			var siteId = Database.CurrentSiteId();

			WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new[] { siteId }))
				[siteId].Should().Be(1);
		}




		[Test]
		public void ShouldLoadForSkill()
		{
			Database
				.WithAgent()
				.WithSkill("phone")
				.UpdateGroupings();
			var siteId = Database.CurrentSiteId();
			var skillId = Database.SkillIdFor("phone");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { siteId }, new[] { skillId }));

			result[siteId].Should().Be(1);
		}

		[Test]
		public void ShouldNotReturnSiteWithoutAgents()
		{
			WithUnitOfWork.Get(() => Target.ForSkills(new[] {Guid.NewGuid()}, new[] {Guid.NewGuid()}))
				.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldOnlyCountAgentOnceForSkill()
		{
			Database
				.WithAgent()
				.WithSkill("phone")
				.WithSkill("email")
				.UpdateGroupings();
			var siteId = Database.CurrentSiteId();
			var phoneId = Database.SkillIdFor("phone");
			var emailId = Database.SkillIdFor("email");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { siteId }, new[] { phoneId, emailId }));

			result[siteId].Should().Be(1);
		}
		
		[Test]
		public void ShouldNotCountFutureOrPastPersonPeriodForSkill()
		{
			Now.Is("2016-11-16");
			Database
				.WithPerson("ashley")
				.WithPersonPeriod("2016-09-01")
				.WithSkill("email")
				.WithPersonPeriod("2016-11-01")
				.WithSkill("phone")
				.WithPersonPeriod("2017-01-01")
				.WithSkill("email")
				.UpdateGroupings()
				;
			var siteId = Database.CurrentSiteId();
			var emailId = Database.SkillIdFor("email");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { siteId }, new[] { emailId }));

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotCountAllPersonPeriodsThatShareSkill()
		{
			Now.Is("2016-11-16");
			Database
				.WithPerson("ashley")
				.WithPersonPeriod("2016-09-01")
				.WithSkill("email")
				.WithPersonPeriod("2016-11-01")
				.WithSkill("email")
				.WithPersonPeriod("2017-01-01")
				.WithSkill("email")
				.UpdateGroupings()
				;
			var siteId = Database.CurrentSiteId();
			var emailId = Database.SkillIdFor("email");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { siteId }, new[] { emailId }));

			result[siteId].Should().Be(1);
		}


		[Test]
		public void ShouldNotLoadTerminatedAgentForSkill()
		{
			Now.Is("2016-11-16 08:00");
			Database
				.WithAgent()
				.WithSkill("phone")
				.WithTerminatedAgent("2016-11-15")
				.WithSkill("phone")
				.UpdateGroupings();
			var siteId = Database.CurrentSiteId();
			var phoneId = Database.SkillIdFor("phone");

			var result = WithUnitOfWork.Get(() => Target.ForSkills(new[] { siteId }, new[] { phoneId }));

			result[siteId].Should().Be(1);
		}


		[Test]
		public void ShouldGetForSkillWithCorrectDate()
		{
			Now.Is("2017-02-21 16:00");
			Database
				.WithPerson()
				.WithPersonPeriod("2017-01-01")
				.WithSkill("phone")
				.WithPersonPeriod("2017-02-22")
				.WithSkill("phone")
				.UpdateGroupings();
			var siteId = Database.CurrentSiteId();
			var phoneId = Database.SkillIdFor("phone");

			WithUnitOfWork.Get(() => Target.ForSkills(new[] {siteId}, new[] {phoneId}))
				[siteId].Should().Be(1);
		}

	}
}