using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class BootstrapperTest
	{
		private Bootstrapper target;

		[SetUp]
		public void Setup()
		{
			target = new Bootstrapper();
		}

		[Test]
		public void AllRegisteredTasksShouldBeExecuted()
		{
			Task1.IsExecuted.Should().Be.False();
			Task2.IsExecuted.Should().Be.False();
			target.Run(new IBootstrapperTask[]{new Task1(), new Task2()});
			Task1.IsExecuted.Should().Be.True();
			Task2.IsExecuted.Should().Be.True();
		}


		[TaskPriority(13)]
		private class Task1 : IBootstrapperTask
		{
			public static bool IsExecuted { get; private set; }

			public void Execute()
			{
				IsExecuted = true;
			}
		}

		[TaskPriority(17)]
		private class Task2 : IBootstrapperTask
		{
			public static bool IsExecuted { get; private set; }

			public void Execute()
			{
				IsExecuted = true;
			}
		}
	}
}