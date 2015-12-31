using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public sealed class ReadOnlyUnitOfWorkAttribute : AspectAttribute
	{
		public ReadOnlyUnitOfWorkAttribute()
			: base(typeof(IReadOnlyUnitOfWorkAspect))
		{
		}
	}
}