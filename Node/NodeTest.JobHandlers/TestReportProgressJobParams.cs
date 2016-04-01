namespace NodeTest.JobHandlers
{
	public class TestReportProgressJobParams
	{
		public TestReportProgressJobParams(string dummy,
		                                   string name)
		{
			Name = name;
			Dummy = dummy;
		}

		public string Dummy { get; private set; }
		public string Name { get; private set; }
	}
}