using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class AllBusinessUnitsUnitOfWorkAttribute : AspectAttribute
	{
		public AllBusinessUnitsUnitOfWorkAttribute() : base(typeof(IAllBusinessUnitsUnitOfWorkAspect))
		{
		}
	}
}