using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class AnalyticsDataFactory
	{
		public ICurrentAnalyticsUnitOfWork CurrentAnalyticsUnitOfWork;
		private readonly IList<IAnalyticsDataSetup> _analyticsSetups = new List<IAnalyticsDataSetup>();
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
				_analyticsSetups.ForEach(s => s.Apply(connection, culture, swedishCulture));
			}
		}

		public IEnumerable<IAnalyticsDataSetup> Setups => _analyticsSetups;
	}
}