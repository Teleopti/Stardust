using MbCache.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class TeleoptiCacheKey : ToStringMbCacheKey
	{
		protected override string ParameterValue(object parameter)
		{
			var entity = parameter as IEntity;
			if (entity == null)
				return base.ParameterValue(parameter);
			return entity.Id.HasValue ? entity.Id.Value.ToString() : null;
		}
	}
}