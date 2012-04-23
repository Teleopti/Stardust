using System;

namespace Teleopti.Ccc.Web.Core.Aop.Core
{
	public class AspectAttribute : Attribute, IAspectAttribute
	{
		public int Order { get; set; }
		public virtual void OnBeforeInvokation() { }
		public virtual void OnAfterInvokation() { }
	}
}