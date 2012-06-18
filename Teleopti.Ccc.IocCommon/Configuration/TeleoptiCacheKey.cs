using System.Globalization;
using MbCache.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class TeleoptiCacheKey : ToStringCacheKey
	{
		protected override string ParameterValue(object parameter)
		{
			var entity = parameter as IEntity;
			var restriction = parameter as IEffectiveRestriction;
			if(entity!=null)
				return entity.Id.HasValue ? entity.Id.Value.ToString() : null;
			if (restriction != null)
				return restriction.GetHashCode().ToString(CultureInfo.InvariantCulture); //this one is WRONG! but it has been wrong for a long time...
			
			return base.ParameterValue(parameter);
		}
	}
}