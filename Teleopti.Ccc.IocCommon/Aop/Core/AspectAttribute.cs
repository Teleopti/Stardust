using System;

namespace Teleopti.Ccc.IocCommon.Aop.Core
{
	public abstract class AspectAttribute : Attribute, IAspectAttribute
	{
		public int Order { get; set; }
		public virtual void OnBeforeInvokation() { }
		public virtual void OnAfterInvokation() { }
	}
}