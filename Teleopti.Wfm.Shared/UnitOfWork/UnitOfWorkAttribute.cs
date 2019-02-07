using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public sealed class UnitOfWorkAttribute : AspectAttribute
	{
		public UnitOfWorkAttribute() : base(typeof(IUnitOfWorkAspect))
		{
		}
	}
}