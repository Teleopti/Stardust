using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[InfrastructureTest]
	public class FindPersonAssignmentAsyncTest
	{
		public IFindPersonAssignment Target;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		[Test]
		public void ShouldFindByAgent()
		{
			var agentToFind = new Person().InTimeZone(TimeZoneInfo.Utc);
			var agentNotToFind = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var assToFind = new PersonAssignment(agentToFind, scenario, period.StartDate);
			var assNotToFind = new PersonAssignment(agentNotToFind, scenario, period.StartDate);

			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				setup.FetchSession().Save(agentToFind);
				setup.FetchSession().Save(agentNotToFind);
				setup.FetchSession().Save(scenario);
				setup.FetchSession().Save(assToFind);
				setup.FetchSession().Save(assNotToFind);
				setup.PersistAll();
			}

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				Target.Find(new[] {agentToFind}, period, scenario).Result.Single()
					.Should().Be.EqualTo(assToFind);				
			}
		}
		
		[Test]
		public void ShouldFindByScenario()
		{
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenarioToFind = new Scenario();
			var scenarioNotToFind = new Scenario();
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var assToFind = new PersonAssignment(agent, scenarioToFind, period.StartDate);
			var assNotToFind = new PersonAssignment(agent, scenarioNotToFind, period.StartDate);

			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				setup.FetchSession().Save(agent);
				setup.FetchSession().Save(scenarioToFind);
				setup.FetchSession().Save(scenarioNotToFind);
				setup.FetchSession().Save(assToFind);
				setup.FetchSession().Save(assNotToFind);
				setup.PersistAll();
			}

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				Target.Find(new[] {agent}, period, scenarioToFind).Result.Single()
					.Should().Be.EqualTo(assToFind);				
			}
		}
		
		[Test]
		public void ShouldFindByPeriod()
		{
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var assToFind = new PersonAssignment(agent, scenario, period.StartDate);
			var assNotToFind1 = new PersonAssignment(agent, scenario, period.StartDate.AddDays(-1));
			var assNotToFind2 = new PersonAssignment(agent, scenario, period.StartDate.AddDays(1));

			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				setup.FetchSession().Save(agent);
				setup.FetchSession().Save(scenario);
				setup.FetchSession().Save(assToFind);
				setup.FetchSession().Save(assNotToFind1);
				setup.FetchSession().Save(assNotToFind2);
				setup.PersistAll();
			}

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				Target.Find(new[] {agent}, period, scenario).Result.Single()
					.Should().Be.EqualTo(assToFind);				
			}
		}
	}
}