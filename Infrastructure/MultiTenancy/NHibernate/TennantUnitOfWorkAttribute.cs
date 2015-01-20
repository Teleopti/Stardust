using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public class TennantUnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public TennantUnitOfWorkAttribute() : base(typeof(TennantUnitOfWorkAspect))
		{
		}
	}
}