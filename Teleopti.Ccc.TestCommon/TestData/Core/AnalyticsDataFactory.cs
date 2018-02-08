using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class AnalyticsDataFactory
	{
		private static readonly CultureInfo swedishCulture = CultureInfo.GetCultureInfo("sv-SE");
		
		public void Apply(IAnalyticsDataSetup analyticsDataSetup)
		{
			using (var connection = new SqlConnection(InfraTestConfigReader.AnalyticsConnectionString))
			{
				var culture = Thread.CurrentThread.CurrentCulture;
				connection.Open();
				analyticsDataSetup.Apply(connection, culture, swedishCulture);
			}
		}
		
		
		
		

		private readonly IList<IAnalyticsDataSetup> _setups = new List<IAnalyticsDataSetup>();
		public void Setup(IAnalyticsDataSetup setup) => _setups.Add(setup);
		public void Persist() { Persist(Thread.CurrentThread.CurrentCulture); }
		public void Persist(CultureInfo culture)
		{
			using (var connection = new SqlConnection(InfraTestConfigReader.AnalyticsConnectionString))
			{
				connection.Open();
				_setups.ForEach(s => s.Apply(connection, culture, swedishCulture));
			}
		}

		public IEnumerable<IAnalyticsDataSetup> Setups => _setups;
	}
}