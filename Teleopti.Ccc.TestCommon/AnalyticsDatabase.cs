using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon
{
	public class AnalyticsDatabase
	{
		private readonly IConnectionStrings _connectionStrings;

		public int CurrentDataSourceId;

		public AnalyticsDatabase(IConnectionStrings connectionStrings)
		{
			_connectionStrings = connectionStrings;
		}

		[AnalyticsUnitOfWork]
		public virtual void WithDataSource(int datasourceId, string sourceId)
		{
			CurrentDataSourceId = datasourceId;
			// really, shouldnt make 2 connections!
			using (var connection = new SqlConnection(_connectionStrings.Analytics()))
			{
				connection.Open();
				using (var table = sys_datasource.CreateTable())
				{
					table.AddDataSource(CurrentDataSourceId, " ", -1, " ", -1, " ", " ", 1, false, sourceId, false);
					Bulk.Insert(connection, table);
				}
			}
		}

		[AnalyticsUnitOfWork]
		public virtual AnalyticsDatabase WithEternityAndNotDefinedDate()
		{
			return apply(new EternityAndNotDefinedDate());
		}

		[AnalyticsUnitOfWork]
		public virtual AnalyticsDatabase WithQuarterOfAnHourInterval()
		{
			return apply(new QuarterOfAnHourInterval());
		}

		[AnalyticsUnitOfWork]
		public virtual AnalyticsDatabase WithDefaultSkillset()
		{
			return apply(new DefaultSkillset());
		}

		[AnalyticsUnitOfWork]
		public virtual AnalyticsDatabase WithDefaultAcdLogin()
		{
			return apply(new DefaultAcdLogin());
		}

		private AnalyticsDatabase apply(IAnalyticsDataSetup setup)
		{
			using (var connection = new SqlConnection(_connectionStrings.Analytics()))
			{
				connection.Open();
				setup.Apply(connection, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture);
			}
			return this;
		}
	}
}