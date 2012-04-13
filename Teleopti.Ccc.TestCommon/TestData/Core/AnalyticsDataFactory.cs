﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class AnalyticsDataFactory
	{
		private readonly IList<IAnalyticsDataSetup> _analyticsSetups = new List<IAnalyticsDataSetup>();
		
		public void Setup(IAnalyticsDataSetup analyticsDataSetup)
		{
			_analyticsSetups.Add(analyticsDataSetup);
		}

		public void Persist() { Persist(Thread.CurrentThread.CurrentCulture); }

		public void Persist(CultureInfo culture)
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
			{
				connection.Open();
				_analyticsSetups.ForEach(s => s.Apply(connection, culture, CultureInfo.GetCultureInfo("sv-SE")));
			}
		}

		private IEnumerable<T> QueryData<T>() { return from s in _analyticsSetups where typeof(T).IsAssignableFrom(s.GetType()) select (T)s; }

		public IEnumerable<IAnalyticsDataSetup> Setups { get { return _analyticsSetups; } } 
		public bool HasSetup<T>() { return QueryData<T>().Any(); }
		public T Data<T>() { return QueryData<T>().SingleOrDefault(); }

	}
}