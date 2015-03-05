using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class AllBusinessUnitsUnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public AllBusinessUnitsUnitOfWorkAttribute()
			: base(typeof(IAllBusinessUnitsUnitOfWorkAspect))
		{
		}
	}
}