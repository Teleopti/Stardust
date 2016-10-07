using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class StardustJobFeedback: IStardustJobFeedback
	{
		public Action<string> SendProgress { get; set; }
	}

	public class FakeStardustJobFeedback : IStardustJobFeedback
	{
		private static void fakeSend(string message)
		{
		}

		public Action<string> SendProgress { get; set; } = fakeSend;
	}
}