using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	public class RetryingQueueSimulator
	{
		private readonly List<Thread> _jobThreads = new List<Thread>();
		public int RetryCount { get; set; }

		public void ProcessAsync(Action job)
		{
			_jobThreads.Add(Execute.OnAnotherThread(() => tryJob(100, job)));
		}

		private void tryJob(int retriesLeft, Action job)
		{
			try
			{
				Exception ex = null;
				Execute.OnAnotherThread(() =>
				{
					try
					{
						job.Invoke();
					}
					catch (Exception e)
					{
						ex = e;
					}
				})
					.Join();
				if (ex != null)
					throw ex;
			}
			catch(Exception ex)
			{
				RetryCount++;
				if (retriesLeft == 0)
				{
					Debug.WriteLine(ex.ToString());
					return;
				}
				retriesLeft--;
				tryJob(retriesLeft, job);
			}
		}

		public void WaitForAll()
		{
			_jobThreads.ForEach(t => t.Join());
		}

	}
}