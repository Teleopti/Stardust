using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class WorkShiftAddStopperCallback : IWorkShiftAddCallback
	{
		private DateTime _timerStart;
		private const int stopSeconds = 100;
		private bool _sentStop;

		public void BeforeAdd(IWorkShift item)
		{
			checkStop();
		}

		public void BeforeRemove()
		{
			checkStop();
		}

		private void checkStop()
		{
			if (_sentStop == false && _timerStart.AddSeconds(stopSeconds) < DateTime.Now)
			{
				_sentStop = true;
				IsCanceled = true;
			}
		}
		public bool IsCanceled { get; private set; }
		
		public void StartNewRuleSet(IWorkShiftRuleSet ruleSet)
		{
			_sentStop = false;
			_timerStart = DateTime.Now;
		}

		public void EndRuleSet()
		{
			_timerStart = DateTime.Now;
			IsCanceled = false;
		}
	}
}