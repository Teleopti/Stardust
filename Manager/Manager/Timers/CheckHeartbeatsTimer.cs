using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using Timer = System.Timers.Timer;

namespace Stardust.Manager.Timers
{
	public class CheckHeartbeatsTimer : Timer
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (CheckHeartbeatsTimer));

		private IWorkerNodeRepository _workerNodeRepository;

		public CheckHeartbeatsTimer(IWorkerNodeRepository workerNodeRepository, double interval = 10000) : base(interval)
		{
			_workerNodeRepository = workerNodeRepository;
			Elapsed += OnTimedEvent;
			AutoReset = true;

			Start();
		}

		private void OnTimedEvent(object sender,
										ElapsedEventArgs e)
		{
				_workerNodeRepository.CheckNodesAreAlive(TimeSpan.FromSeconds(10));
				LogHelper.LogInfoWithLineNumber(Logger, " Check Heartbeat");
		}

	}
}
