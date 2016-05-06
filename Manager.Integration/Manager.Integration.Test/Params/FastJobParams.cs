namespace Manager.Integration.Test.Params
{
	public class FastJobParams
	{
		public string Name { get; private set; }

		public FastJobParams(string name)
		{
			Name = name;
		}
	}
}