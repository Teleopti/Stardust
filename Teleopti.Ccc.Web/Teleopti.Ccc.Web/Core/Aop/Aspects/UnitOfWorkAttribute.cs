using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
    public sealed class UnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public UnitOfWorkAttribute() : base(typeof(UnitOfWorkAspect)) { }
	}
}