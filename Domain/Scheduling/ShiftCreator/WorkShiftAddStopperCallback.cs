using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class WorkShiftAddStopperCallback : IWorkShiftAddCallback
	{
		// not used here this one just cancel the creation when going over 100 sec.
		public event EventHandler<EventArgs> CountChanged;
		public event EventHandler<EventArgs> RuleSetReady;
		public event EventHandler<EventArgs> RuleSetWarning;
		public event EventHandler<ComplexRuleSetEventArgs> RuleSetToComplex;

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
		public void Cancel()
		{
			IsCanceled = true;
		}

		public int CurrentCount { get; private set; }
		public string CurrentRuleSetName { get; private set; }

		public void StartNewRuleSet(IWorkShiftRuleSet ruleSet)
		{
			_sentStop = false;
			_timerStart = DateTime.Now;
			CurrentCount = 0;
			CurrentRuleSetName = ruleSet.Description.Name;
		}

		public void EndRuleSet()
		{
			_timerStart = DateTime.Now;
			IsCanceled = false;
		}
	}
}