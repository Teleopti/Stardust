using Teleopti.Ccc.IocCommon.Aop.Core;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
    public sealed class UnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public UnitOfWorkAttribute() : base(typeof(UnitOfWorkAspect)) { }
	}
}