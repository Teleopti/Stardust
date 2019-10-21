namespace NodeTest.JobHandlers
{
	public class CrashingJobParams
	{
		public CrashingJobParams(string error)
		{
			Error = error;
		}

		public string Error { get; private set; }
	}
}