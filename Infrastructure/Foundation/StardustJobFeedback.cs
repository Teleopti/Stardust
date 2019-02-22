using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class StardustJobFeedback: IStardustJobFeedback
	{
		public Action<string> SendProgress { get; set; }
	}

	public class FakeStardustJobFeedback : IStardustJobFeedback
	{
		public static readonly List<string> FeedbackList = new List<string>();
		
		private static void fakeSend(string message)
		{
			Console.WriteLine(message);
			FeedbackList.Add(message);
		}

		public Action<string> SendProgress { get; set; } = fakeSend;
	}

	public class EmptyStardustJobFeedback : IStardustJobFeedback
	{
		private static void fakeSend(string message)
		{
		}

		public Action<string> SendProgress { get; set; } = fakeSend;
	}
}