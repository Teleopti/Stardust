using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using NodeTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Node;
using Stardust.Node.Workers;

namespace NodeTest
{
	[TestFixture]
	public class JobDetailSenderTest
	{
		private NodeConfiguration _nodeConfiguration;
		private FakeHttpSender _httpSenderFake;
		public JobDetailSender Target;

		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			_nodeConfiguration = new NodeConfiguration(
				new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
				Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
				14100,
				"TestNode",
				60,
				100, true);
			_httpSenderFake = new FakeHttpSender();
		}

		[Test]
		public void ShouldBeAbleToAddDetail()
		{
			Target = new JobDetailSender(_httpSenderFake);
			Target.AddDetail(Guid.NewGuid(), "Progress message");
			Assert.IsTrue(Target.DetailsCount() == 1);
		}

		[Test]
		public void ShouldHaveTwoJobProgressesWhenTwoWithSameGuidAreAdded()
		{
			Target = new JobDetailSender(_httpSenderFake);

			var jobid = Guid.NewGuid();

			Target.AddDetail(jobid, "Progress message 1.");
			Target.AddDetail(jobid, "Progress message 2.");
			Assert.IsTrue(Target.DetailsCount() == 2);
		}

		[Test]
		public void ShouldRemoveDetailWhenSent()
		{
			Target = new JobDetailSender(_httpSenderFake);
            Target.SetManagerLocation(_nodeConfiguration.ManagerLocation);
			Target.AddDetail(Guid.NewGuid(), "Progress message.");
			Target.Send(CancellationToken.None);
			Target.DetailsCount().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSendAllDetailsInTheSameRequest()
		{
			Target = new JobDetailSender(_httpSenderFake);
            Target.SetManagerLocation(_nodeConfiguration.ManagerLocation);
            var jobid = Guid.NewGuid();
			Target.AddDetail(jobid, "Progress message 1.");
			Target.AddDetail(jobid, "Progress message 2.");

			Target.Send(CancellationToken.None);
			_httpSenderFake.SentJson.Should().Contain("Progress message 1.");
			_httpSenderFake.SentJson.Should().Contain("Progress message 2.");
		}


	}
}
