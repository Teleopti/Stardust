using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class StardustJobFeedback: IStardustJobFeedback
	{
		public Action<string> SendProgress { get; set; }
	}
}