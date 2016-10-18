using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[InfrastructureTest]
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
				.WithTerminatedAgent("2016-10-17");
			var siteId = Database.CurrentSiteId();

			var result = WithUnitOfWork.Get(() => Target.FetchNumberOfAgents(new[] { siteId }));

			result[siteId].Should().Be(1);
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
	}
}