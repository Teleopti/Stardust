using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Logon.Aspects
{
	public class DataSourceAttribute : AspectAttribute
	{
		public DataSourceAttribute() : base(typeof(IDataSourceAspect))
		{
		}
	}
}