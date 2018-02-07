using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public sealed class ReadonlyUnitOfWorkAttribute : AspectAttribute
	{
		public ReadonlyUnitOfWorkAttribute() : base(typeof(IUnitOfWorkNoCommitAspect))
		{
		}
	}
}