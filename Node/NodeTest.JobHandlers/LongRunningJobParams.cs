namespace NodeTest.JobHandlers
{
	public class LongRunningJobParams
	{
		public LongRunningJobParams(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}