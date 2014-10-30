using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public ReadModelUnitOfWorkAttribute()
			: base(typeof(ReadModelUnitOfWorkAspect))
		{
		}
	}
}