using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class AnalyticsDataFactory
	{
		private readonly IList<IAnalyticsDataSetup> _analyticsSetups = new List<IAnalyticsDataSetup>();

		public void Apply(IAnalyticsDataSetup analyticsDataSetup)
		{
			Bulk.Retrying(InfraTestConfigReader.AnalyticsConnectionString, connection =>
			{
				var culture = Thread.CurrentThread.CurrentCulture;
				analyticsDataSetup.Apply(connection, culture, CultureInfo.GetCultureInfo("sv-SE"));
			});
		}

		public void Setup(IAnalyticsDataSetup analyticsDataSetup)
		{
			_analyticsSetups.Add(analyticsDataSetup);
		}

		public void Persist() { Persist(Thread.CurrentThread.CurrentCulture); }

		public void Persist(CultureInfo culture)
		{
			_analyticsSetups.ForEach(s =>
			{
				Bulk.Retrying(InfraTestConfigReader.AnalyticsConnectionString, connection =>
				{
					s.Apply(connection, culture, CultureInfo.GetCultureInfo("sv-SE"));
				});
			});
		}

		public IEnumerable<IAnalyticsDataSetup> Setups { get { return _analyticsSetups; } } 

	}
}