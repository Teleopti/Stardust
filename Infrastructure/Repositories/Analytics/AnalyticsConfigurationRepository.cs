using System.Globalization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsConfigurationRepository : IAnalyticsConfigurationRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsConfigurationRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public CultureInfo GetCulture()
		{
			var culture = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"select [value] from [mart].[sys_configuration]
					where [key] = 'Culture'").UniqueResult<string>();
			int cultureId;
			if (string.IsNullOrEmpty(culture) || !int.TryParse(culture, out cultureId))
				return CultureInfo.CurrentCulture;
			return CultureInfo.GetCultureInfo(cultureId);
		}
	}
}