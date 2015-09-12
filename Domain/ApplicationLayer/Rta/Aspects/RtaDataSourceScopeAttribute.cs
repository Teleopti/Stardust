using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects
{
	public class RtaDataSourceScopeAttribute : AspectAttribute
	{
		public RtaDataSourceScopeAttribute() : base(typeof(IRtaDataSourceScope))
		{
		}
	}
}