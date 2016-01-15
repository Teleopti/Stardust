using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public abstract class AspectAttribute : Attribute
	{
		protected AspectAttribute(Type aspectType)
		{
			AspectType = aspectType;
		}

		public Type AspectType { get; private set; }

		public virtual int Order
		{
			get { return 0; }
		}
	}
}