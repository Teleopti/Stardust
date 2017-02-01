using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.TestCommon
{
	public class TrackSchedulingProgress<T> : ISchedulingProgress
	{
		public TrackSchedulingProgress()
		{
			ReportedProgress = new List<T>();
		}

		public ICollection<T> ReportedProgress { get; }

		public bool CancellationPending => false;

		public void ReportProgress(int percentProgress, object userState = null)
		{
			ReportedProgress.Add((T) userState);
		}
	}
}