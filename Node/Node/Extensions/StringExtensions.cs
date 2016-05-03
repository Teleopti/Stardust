using System;

namespace Stardust.Node.Extensions
{
	public static class StringExtensions
	{

		public static void ThrowArgumentNullExceptionIfNullOrEmpty(this string stringValue)
		{
			if (string.IsNullOrEmpty(stringValue))
			{
				throw new ArgumentNullException();
			}
		}
	}
}