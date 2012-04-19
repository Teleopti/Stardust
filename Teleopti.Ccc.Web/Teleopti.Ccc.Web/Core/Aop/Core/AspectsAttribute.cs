using AutofacContrib.DynamicProxy2;

namespace Teleopti.Ccc.Web.Core.Aop.Core
{
	public class AspectsAttribute : InterceptAttribute
	{
		public AspectsAttribute() : base(typeof(AspectInterceptor)) { }
	}
}