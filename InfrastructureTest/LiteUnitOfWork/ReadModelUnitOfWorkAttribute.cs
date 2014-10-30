using Teleopti.Ccc.IocCommon.Aop.Core;

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