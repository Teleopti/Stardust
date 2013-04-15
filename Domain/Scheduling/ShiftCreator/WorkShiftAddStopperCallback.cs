using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class WorkShiftAddStopperCallback : IWorkShiftAddCallback
	{
		private const int maxCount = 500000;
		private bool _sentStop;
		private int _totalCount;

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
			_totalCount += 1;
			if (_sentStop == false && _totalCount > maxCount)
			{
				_sentStop = true;
				IsCanceled = true;
			}
		}
		public bool IsCanceled { get; private set; }
		
		public void StartNewRuleSet(IWorkShiftRuleSet ruleSet)
		{
			_sentStop = false;
			_totalCount = 0;
		}

		public void EndRuleSet()
		{
			_totalCount = 0;
			IsCanceled = false;
		}
	}
}