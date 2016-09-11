﻿using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class AnalyticsDataFactory
	{
		private readonly IList<IAnalyticsDataSetup> _analyticsSetups = new List<IAnalyticsDataSetup>();

		public void Apply(IAnalyticsDataSetup analyticsDataSetup)
		{
			using (var connection = new SqlConnection(InfraTestConfigReader.AnalyticsConnectionString))
			{
				var culture = Thread.CurrentThread.CurrentCulture;
				connection.Open();
				analyticsDataSetup.Apply(connection, culture, CultureInfo.GetCultureInfo("sv-SE"));
			}
		}

		public void Setup(IAnalyticsDataSetup analyticsDataSetup)
		{
			_analyticsSetups.Add(analyticsDataSetup);
		}

		public void Persist() { Persist(Thread.CurrentThread.CurrentCulture); }

		public void Persist(CultureInfo culture)
		{
			using (var connection = new SqlConnection(InfraTestConfigReader.AnalyticsConnectionString))
			{
				connection.Open();
				_analyticsSetups.ForEach(s => s.Apply(connection, culture, CultureInfo.GetCultureInfo("sv-SE")));
			}
		}

		public IEnumerable<IAnalyticsDataSetup> Setups { get { return _analyticsSetups; } } 

	}
}