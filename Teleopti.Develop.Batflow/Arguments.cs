using CommandLine;

namespace Teleopti.Develop.Batflow
{
	[Verb("flow", HelpText = "Restores baseline for applications and configures test projects. Dont worry about it.")]
	public class FlowVerb
	{
		[Option("develop.baseline",
			Default = "DemoSales2017",
			HelpText = "Development database baseline to restore.")]
		public string DevelopBaseline { get; set; }

		[Option("develop.server",
			Default = ".",
			HelpText = "Development database server.")]
		public string DevelopServer { get; set; }

		[Option("test.baseline",
			Default = null,
			HelpText = "Test database baseline to restore. Builds databases if specified.")]
		public string TestBaseline { get; set; }

		[Option("test.server",
			Default = ".",
			HelpText = "Test database server.")]
		public string TestServer { get; set; }
	}
	
	[Verb("test.init", HelpText = "Initializes databases for test projects.")]
	public class TestInitVerb
	{
		[Option("server",
			Default = ".",
			HelpText = "Database server.")]
		public string Server { get; set; }

		[Option("baseline",
			Default = null,
			HelpText = "Database baseline to restore. Builds databases if specified.")]
		public string Baseline { get; set; }
	}

	[Verb("test.drop", HelpText = "Drops databases initialized for test projects.")]
	public class TestDropVerb
	{
		[Option("server",
			Default = ".",
			HelpText = "Database server.")]
		public string Server { get; set; }
	}
	
}