using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Jobs
{
	[TestFixture]
	public class JobStepCollectionBaseTest
	{
		private IJobParameters _jobParameters;
		private FakeToggleManager _toggleManager;

		[SetUp]
		public void Setup()
		{
			_jobParameters = MockRepository.GenerateMock<IJobParameters>();
			_toggleManager = new FakeToggleManager();
			_jobParameters.Stub(x => x.ToggleManager).Return(_toggleManager);
		}

		[Test]
		public void AddWhenAllDisabled_ShouldAddWhenAllDisabled()
		{
			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAllDisabledTest();

			target.Should().Not.Be.Empty();
			target.Single().GetType().Should().Be.EqualTo(typeof(TestJobStep));
		}

		[Test]
		public void AddWhenAllDisabled_ShouldNotAddWhenOneEnabled()
		{
			_toggleManager.Enable(Toggles.TestToggle2);

			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAllDisabledTest();

			target.Should().Be.Empty();
		}

		[Test]
		public void AddWhenAllDisabled_ShouldNotAddWhenAllEnabled()
		{
			_toggleManager.Enable(Toggles.TestToggle);
			_toggleManager.Enable(Toggles.TestToggle2);

			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAllDisabledTest();

			target.Should().Be.Empty();
		}

		[Test]
		public void AddWhenAnyDisabled_ShouldAddWhenAllDisabled()
		{
			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAnyDisabledTest();

			target.Should().Not.Be.Empty();
			target.Single().GetType().Should().Be.EqualTo(typeof(TestJobStep));
		}

		[Test]
		public void AddWhenAnyDisabled_ShouldAddWhenOneEnabled()
		{
			_toggleManager.Enable(Toggles.TestToggle2);

			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAnyDisabledTest();

			target.Should().Not.Be.Empty();
			target.Single().GetType().Should().Be.EqualTo(typeof(TestJobStep));
		}

		[Test]
		public void AddWhenAnyDisabled_ShouldNotAddWhenAllEnabled()
		{
			_toggleManager.Enable(Toggles.TestToggle);
			_toggleManager.Enable(Toggles.TestToggle2);

			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAnyDisabledTest();

			target.Should().Be.Empty();
		}

		[Test]
		public void AddWhenAllEnabled_ShouldNotAddWhenAllDisabled()
		{
			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAllEnabledTest();

			target.Should().Be.Empty();
			
		}

		[Test]
		public void AddWhenAllEnabled_ShouldNotAddWhenOneEnabled()
		{
			_toggleManager.Enable(Toggles.TestToggle2);

			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAllEnabledTest();

			target.Should().Be.Empty();
		}

		[Test]
		public void AddWhenAllEnabled_ShouldAddWhenAllEnabled()
		{
			_toggleManager.Enable(Toggles.TestToggle);
			_toggleManager.Enable(Toggles.TestToggle2);

			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAllEnabledTest();

			target.Should().Not.Be.Empty();
			target.Single().GetType().Should().Be.EqualTo(typeof(TestJobStep));
		}

		[Test]
		public void AddWhenAnyEnabled_ShouldNotAddWhenAllDisabled()
		{
			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAnyEnabledTest();

			target.Should().Be.Empty();

		}

		[Test]
		public void AddWhenAnyEnabled_ShouldAddWhenOneEnabled()
		{
			_toggleManager.Enable(Toggles.TestToggle2);

			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAnyEnabledTest();

			target.Should().Not.Be.Empty();
			target.Single().GetType().Should().Be.EqualTo(typeof(TestJobStep));
		}

		[Test]
		public void AddWhenAnyEnabled_ShouldAddWhenAllEnabled()
		{
			_toggleManager.Enable(Toggles.TestToggle);
			_toggleManager.Enable(Toggles.TestToggle2);

			var target = new jobStepCollectionBaseTester(_jobParameters);
			target.AddWhenAnyEnabledTest();

			target.Should().Not.Be.Empty();
			target.Single().GetType().Should().Be.EqualTo(typeof(TestJobStep));
		}

		private class jobStepCollectionBaseTester : JobStepCollectionBase
		{
			private readonly IJobParameters _jobParameters;

			public jobStepCollectionBaseTester(IJobParameters jobParameters)
			{
				_jobParameters = jobParameters;
			}

			public void AddWhenAllDisabledTest()
			{
				AddWhenAllDisabled(new TestJobStep(_jobParameters, "Test", true), Toggles.TestToggle, Toggles.TestToggle2);
			}

			public void AddWhenAnyDisabledTest()
			{
				AddWhenAnyDisabled(new TestJobStep(_jobParameters, "Test", true), Toggles.TestToggle, Toggles.TestToggle2);
			}

			public void AddWhenAllEnabledTest()
			{
				AddWhenAllEnabled(new TestJobStep(_jobParameters, "Test", true), Toggles.TestToggle, Toggles.TestToggle2);
			}

			public void AddWhenAnyEnabledTest()
			{
				AddWhenAnyEnabled(new TestJobStep(_jobParameters, "Test", true), Toggles.TestToggle, Toggles.TestToggle2);
			}
		}
	}
}