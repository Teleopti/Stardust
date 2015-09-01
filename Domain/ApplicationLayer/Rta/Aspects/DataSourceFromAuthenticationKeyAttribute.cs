using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects
{
	public class DataSourceFromAuthenticationKeyAttribute : AspectAttribute
	{
		public DataSourceFromAuthenticationKeyAttribute() : base(typeof(IDataSourceFromAuthenticationKeyAspect))
		{
		}
	}
}