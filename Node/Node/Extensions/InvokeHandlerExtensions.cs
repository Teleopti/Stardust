using System;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Extensions
{
	public static class InvokeHandlerExtensions
	{
		public static void ThrowArgumentNullExceptionWhenNull(this IInvokeHandler invokeHandler)
		{
			if (invokeHandler == null)
			{
				throw new ArgumentNullException("invokeHandler");
			}
		}
	}
}