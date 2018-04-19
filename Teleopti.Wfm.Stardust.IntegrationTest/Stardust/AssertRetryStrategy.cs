namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	public class AssertRetryStrategy
	{
		private readonly int _numberOfTries;
		private int _currentTry;

		public AssertRetryStrategy(int numberOfTries)
		{
			_numberOfTries = numberOfTries;
			_currentTry = 0;
		}

		public void Reset()
		{
			_currentTry = 0;
		}

		public bool TryAgain()
		{
			_currentTry++;
			if (_currentTry <= _numberOfTries)
				return true;
			return false;
		}

		public bool WithinRetryStrategy()
		{
			if (_currentTry <= _numberOfTries)
				return true;
			return false;
		}
	}
}