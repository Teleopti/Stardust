using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts
{
	public class WorkShiftAddCallback : IWorkShiftAddCallbackWithEvent
	{
		public event EventHandler<EventArgs> CountChanged;
		public event EventHandler<EventArgs> RuleSetReady;
		public event EventHandler<EventArgs> RuleSetWarning;
		public event EventHandler<ComplexRuleSetEventArgs> RuleSetToComplex;

		private int _updateCount;
		private DateTime _timerStart;
		private const int warningSeconds = 3;
		private const int maxCount = 500000;
		private bool _sentWarning;
		private bool _sentStop;
		private int _totalCount;

		public void BeforeAdd(IWorkShift item)
		{
			_totalCount += 1;
			CurrentCount += 1;
			sendEvent();
			Application.DoEvents();
		}

		public void BeforeRemove()
		{
			_totalCount += 1;
			CurrentCount -= 1;
			sendEvent();
			Application.DoEvents();
		}

		private void sendEvent()
		{
			_updateCount += 1;
			if (_updateCount < 50) return;
			_updateCount = 0;

			checkWarning();

			if (CountChanged != null)
				CountChanged.Invoke(this, new EventArgs());
		}

		private void checkWarning()
		{
			if (_sentWarning == false &&  _timerStart.AddSeconds(warningSeconds) < DateTime.Now)
			{
				_sentWarning = true;
				if (RuleSetWarning != null)
					RuleSetWarning.Invoke(this, new EventArgs());
			}
			if (_sentStop == false && _totalCount > maxCount)
			{
				_sentStop = true;
				IsCanceled = true;
				if (RuleSetToComplex != null)
					RuleSetToComplex.Invoke(this, new ComplexRuleSetEventArgs(CurrentRuleSetName));
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
			_sentWarning = false;
			_sentStop = false;
			_timerStart = DateTime.Now;
			_totalCount = 0;
			CurrentCount = 0;
			if (ruleSet != null)
				CurrentRuleSetName = ruleSet.Description.Name;
		}

		public void EndRuleSet()
		{
			if (RuleSetReady != null)
				RuleSetReady.Invoke(this, new EventArgs());
			_timerStart = DateTime.Now;
			IsCanceled = false;
		}
	}
}