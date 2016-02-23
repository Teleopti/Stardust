namespace Manager.Integration.Test
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