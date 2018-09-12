﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain
{
	[TestFixture]
	[PrincipalAndStateTest]
	[Toggle(Toggles.RTA_ReviewHistoricalAdherence_74770)]
	public class RtaEventStoreSynchronizerTest
	{
		public MutableNow Now;
		public Database Database;
		public IRtaEventStoreSynchronizer Synchronizer;
		public IEventPublisher Publisher;
		public IPrincipalAndStateContext Context;
		public ICurrentDataSource DataSource;
		public IDataSourceScope DataSourceScope;
		public HistoricalOverviewViewModelBuilder Target;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldSynchronize()
		{
			Now.Is("2018-09-06 14:00");
			var dataSource = DataSource.Current();
			Database.WithAgent();
			var personId = Database.CurrentPersonId();
			Context.Logout();
			using (DataSourceScope.OnThisThreadUse(dataSource))
				Publisher.Publish(new PersonStateChangedEvent
				{
					PersonId = personId,
					Timestamp = Now.UtcDateTime(),
				});

			using (DataSourceScope.OnThisThreadUse(dataSource))
				Assert.DoesNotThrow(() => { Synchronizer.Synchronize(); });
		}
		
		
		[Test]
		public void ShouldSynchronizeAdherence()
		{			
			Now.Is("2018-09-10 08:00");
			var dataSource = DataSource.Current();
			Database
				.WithAgent()
				.WithActivity("phone")
				.WithAssignment("2018-09-10")
				.WithAssignedActivity("phone", "2018-09-10 08:00", "2018-09-10 17:00");			
			var personId = Database.CurrentPersonId();
			var teamId = Database.CurrentTeamId();
			Context.Logout();
			using (DataSourceScope.OnThisThreadUse(dataSource))
			{
				Publisher.Publish(new PersonStateChangedEvent
				{
					PersonId = personId,
					Timestamp = Now.UtcDateTime(),
					ActivityName = "phone",
					Adherence = EventAdherence.In
				});
				
				Synchronizer.Synchronize();
			}						
			Now.Is("2018-09-11 08:00");			
			Context.Login();
			
			var data = UnitOfWork.Get(() => Target.Build(null, new[] {teamId}).First());
			
			data.Agents.Single().Days.Last().Adherence.Should().Be(100);
		}
	}
}