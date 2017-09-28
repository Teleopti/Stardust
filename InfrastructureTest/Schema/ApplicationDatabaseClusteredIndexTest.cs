using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Schema
{
	[InfrastructureTest]
	public class ApplicationDatabaseClusteredIndexTest
	{
		public WithUnitOfWork ApplicationDatabase;
		public WithAnalyticsUnitOfWork AnalyticsDatabase;

		[Test]
		public void WarnIfAccidentallyAddingNewTablesWithoutClusteredIndexInApplicationDatabase()
		{
			var exceptions = new[]
			{
				"dbo.ApplicationRolesForSeat",
				"dbo.OutboundCampaignWorkingHours"
			};
			var tables = ApplicationDatabase.Get(uow => uow.Current().FetchSession()
				.CreateSQLQuery(@"
						SELECT
						DISTINCT
						s.name + '.' + o.name
						FROM sys.objects o
						LEFT JOIN sys.schemas s ON  s.schema_id = o.schema_id
						WHERE 
						o.type_desc = 'USER_TABLE'
						AND NOT EXISTS
						(
							SELECT * FROM sys.indexes ii 
							WHERE ii.object_id = o.object_id
							AND ii.type_desc = 'CLUSTERED'
						)
						")
				.List<string>());

			tables.Except(exceptions)
				.Should().Be.Empty();
		}

		[Test]
		public void WarnIfAccidentallyAddingNewTablesWithoutClusteredIndexInAnalyticsDatabase()
		{
			var exceptions = new[]
			{
				"msg.MailboxNotification",
				"dbo.RtaTracer"
			};
			var tables = AnalyticsDatabase.Get(uow => uow.Current().FetchSession()
				.CreateSQLQuery(@"
						SELECT
						DISTINCT
						s.name + '.' + o.name
						FROM sys.objects o
						LEFT JOIN sys.schemas s ON  s.schema_id = o.schema_id
						WHERE 
						o.type_desc = 'USER_TABLE'
						AND NOT EXISTS
						(
							SELECT * FROM sys.indexes ii 
							WHERE ii.object_id = o.object_id
							AND ii.type_desc = 'CLUSTERED'
						)
						")
				.List<string>());

			tables.Except(exceptions)
				.Should().Be.Empty();
		}

	}
}
