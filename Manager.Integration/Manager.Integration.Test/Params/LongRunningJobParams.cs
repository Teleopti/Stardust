namespace Manager.Integration.Test.TestParams
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