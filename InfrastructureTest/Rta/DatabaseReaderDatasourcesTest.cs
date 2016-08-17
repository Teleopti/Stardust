using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class DatabaseReaderDatasourcesTest
	{
		public IDatabaseReader Reader;
		public MutableNow Now;
		public WithAnalyticsUnitOfWork UnitOfWork;

		[Test]
		public void ShouldLoadDatasources()
		{
			UnitOfWork.Do(uow =>
			{
				uow.Current().FetchSession().CreateSQLQuery(@"SET IDENTITY_INSERT mart.sys_datasource ON").ExecuteUpdate();
				uow.Current().FetchSession()
					.CreateSQLQuery(@"
					INSERT INTO mart.sys_datasource 
					(
						datasource_id,
						datasource_name,
						log_object_id,
						log_object_name,
						datasource_database_id,
						datasource_database_name,
						datasource_type_name,
						time_zone_id,
						inactive,
						insert_date,
						update_date,
						source_id,
						internal
					) VALUES (
						7,
						'',
						0,
						'',
						0,
						'',
						'',
						0,
						0,
						'2016-08-17',
						'2016-08-17',
						'source_id',
						0
					)")
					.ExecuteUpdate();
			});

			var result = UnitOfWork.Get(() => Reader.Datasources());

			result.Should().Contain(new KeyValuePair<string, int>("source_id", 7));
		}

	}
}