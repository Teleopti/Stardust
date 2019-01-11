using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public class UpdateTogglesAttribute : AspectAttribute
	{
		public UpdateTogglesAttribute() : base(typeof(IUpdateTogglesAspect))
		{
		}
	}
	
	public interface IUpdateTogglesAspect : IAspect{}
}