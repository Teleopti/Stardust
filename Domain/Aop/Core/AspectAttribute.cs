using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public abstract class AspectAttribute : Attribute, IAttributeForAspect
	{
		protected AspectAttribute(Type aspectType)
		{
			AspectType = aspectType;
		}

		public Type AspectType { get; private set; }
	}
}