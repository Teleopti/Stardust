using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
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
	}
}