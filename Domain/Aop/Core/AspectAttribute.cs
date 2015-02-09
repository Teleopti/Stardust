using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public abstract class AspectAttribute : Attribute, IAspectAttribute
	{
		public int Order { get; set; }
		public virtual void OnBeforeInvocation() { }
		public virtual void OnAfterInvocation(Exception exception) { }
	}
}