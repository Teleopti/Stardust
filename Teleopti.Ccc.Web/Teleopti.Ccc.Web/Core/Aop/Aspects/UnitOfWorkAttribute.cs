using Teleopti.Ccc.Web.Core.Aop.Core;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
	public class UnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public UnitOfWorkAttribute() : base(typeof(UnitOfWorkAspect)) { }
	}
}