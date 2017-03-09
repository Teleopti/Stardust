using System;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public class AspectApplicationException : Exception
	{
		public AspectApplicationException(string message) : base(message)
		{
		}
	}
}