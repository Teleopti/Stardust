using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Start.Core.Config;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core.Config
{
	public class SharedSettingsFactoryTest
	{
		[Test]
		public void ShouldGetMessageBroker()
		{
			var expected = RandomName.Make();
			var appConfig = new FakeConfigReader();
			appConfig.FakeSetting("MessageBroker", expected);
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService());

			var result = target.Create();

			result.MessageBroker.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetNumberOfDaysToShowNonPendingRequests()
		{
			const int expected = 123;
			var appConfig = new FakeConfigReader();
			appConfig.FakeSetting("NumberOfDaysToShowNonPendingRequests", expected.ToString());
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService());

			var result = target.Create();

			result.NumberOfDaysToShowNonPendingRequests.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetMessageBrokerLongPolling()
		{
			var expected = RandomName.Make();
			var appConfig = new FakeConfigReader();
			appConfig.FakeSetting("MessageBrokerLongPolling", expected);
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService());

			var result = target.Create();

			result.MessageBrokerLongPolling.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetRtaPollingInterval()
		{
			var expected = RandomName.Make();
			var appConfig = new FakeConfigReader();
			appConfig.FakeSetting("RtaPollingInterval", expected);
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService());

			var result = target.Create();

			result.RtaPollingInterval.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetQueue()
		{
			var expected = RandomName.Make();
			var appConfig = new FakeConfigReader();
			appConfig.FakeConnectionString("Queue", expected);
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService());

			var result = target.Create();

			var decryptedResult = Encryption.DecryptStringFromBase64(result.Queue, EncryptionConstants.Image1, EncryptionConstants.Image2);
			decryptedResult.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetHangfireConnectionString()
		{
			var expected = RandomName.Make();
			var appConfig = new FakeConfigReader();
			appConfig.FakeConnectionString("Hangfire", expected);
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService());

			var result = target.Create();

			var decryptedResult = Encryption.DecryptStringFromBase64(result.Hangfire, EncryptionConstants.Image1, EncryptionConstants.Image2);
			decryptedResult.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetPasswordPolicy()
		{
			var expected = RandomName.Make();
			var passwordPolicyService = new FakeLoadPasswordPolicyService
			{
				DocumentAsString = expected
			};
			var target = new SharedSettingsFactory(new FakeConfigReader(), passwordPolicyService);

			var result = target.Create();

			result.PasswordPolicy.Should().Be.EqualTo(expected);
		}
	}
}