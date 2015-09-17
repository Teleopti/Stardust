using System.Globalization;
using MbCache.Configuration;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

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
			var entity = parameter as IEntity;
			if(entity!=null)
				return entity.Id.HasValue ? entity.Id.Value.ToString() : null;
			var restriction = parameter as IWorkTimeMinMaxRestriction;
			if (restriction != null)
				return restriction.GetHashCode().ToString(CultureInfo.InvariantCulture); //this one is WRONG! but it has been wrong for a long time...

			//every instance of a callback should be treated as same parameter regarding caching.
			//don't know if it's correct but it is how it worked since 2013 ;)
			//give it a hard coded name to get rid of log4net warnings.
			var workshiftCallback = parameter as IWorkShiftAddCallback;
			if (workshiftCallback != null)
				return "callback"; 
			
			return base.ParameterValue(parameter);
		}
	}
}