using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class ReadModelUnitOfWorkAttribute : AspectAttribute
	{
		public ReadModelUnitOfWorkAttribute()
			: base(typeof(IReadModelUnitOfWorkAspect))
		{
		}
	}
}