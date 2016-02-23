namespace Manager.Integration.Test
{
	public class FailingJobParams
	{
		public FailingJobParams(string error)
		{
			Error = error;
		}

		public string Error { get; private set; }
	}
}