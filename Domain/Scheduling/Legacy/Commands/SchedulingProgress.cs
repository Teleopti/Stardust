using System;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class SchedulingProgress : ISchedulingProgress
	{
		public event EventHandler<FeedbackEventArgs> FeedbackChanged;

		public bool CancellationPending { get; private set; }

		public void ReportProgress(int percentProgress, object userState)
		{
			if(userState == null) return;
			var information = "";
			if (userState.GetType().GetProperty("Message") != null)
			{
				information = (string)userState.GetType().GetProperty("Message").GetValue(userState);
			}
			var jobResultProgress = new JobResultProgress
			{
				Message = information
			};
			OnFeedbackChanged(new FeedbackEventArgs(jobResultProgress));
		}

		protected virtual void OnFeedbackChanged(FeedbackEventArgs e)
		{
			var handler = FeedbackChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}
	}
}