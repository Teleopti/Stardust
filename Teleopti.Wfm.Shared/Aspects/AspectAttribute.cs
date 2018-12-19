using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public abstract class AspectAttribute : Attribute
	{
		protected AspectAttribute(Type aspectType)
		{
			AspectType = aspectType;
		}

		public Type AspectType { get; protected set; }

		public virtual int Order => 0;
	}
}