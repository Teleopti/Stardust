using System.Globalization;
using MbCache.Configuration;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class CachePerDataSource : TeleoptiCacheKey
	{
		private readonly ICurrentDataSource _dataSource;

		public CachePerDataSource(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		protected override string Scope()
		{
			return _dataSource.Current().DataSourceName;
		}
	}

	public class TeleoptiCacheKey : ToStringCacheKey
	{
		protected override string ParameterValue(object parameter)
		{
			switch (parameter)
			{
				case IEntity entity:
					return entity.Id?.ToString();
				case IWorkTimeMinMaxRestriction restriction:
					return restriction.GetHashCode().ToString(CultureInfo.InvariantCulture); //this one is WRONG! but it has been wrong for a long time...
				case IWorkShiftAddCallback _:
					return "callback";
				case IDateOnlyAsDateTimePeriod period:
					return period.DateOnly.ToShortDateString() + period.TimeZone().DisplayName;
			}

			return base.ParameterValue(parameter);
		}
	}
}