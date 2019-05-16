using System;

namespace Stardust.Node.Workers
{
	public interface INow
	{
		DateTime UtcDateTime();
	}

	public sealed class Now : INow
	{
		public DateTime UtcDateTime()
		{
			return DateTime.UtcNow;
		}
	}
}