using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public ReadModelUnitOfWorkAttribute()
			: base(typeof(ReadModelUnitOfWorkAspect))
		{
		}
	}
}