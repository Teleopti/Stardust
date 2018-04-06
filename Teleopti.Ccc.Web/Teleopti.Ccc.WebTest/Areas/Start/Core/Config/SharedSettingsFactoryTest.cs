﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Start.Core.Config;

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
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService(),
				new FakeApplicationInsightsConfigReader());

			var result = target.Create();

			result.MessageBroker.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetNumberOfDaysToShowNonPendingRequests()
		{
			const int expected = 123;
			var appConfig = new FakeConfigReader();
			appConfig.FakeSetting("NumberOfDaysToShowNonPendingRequests", expected.ToString());
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService(),
				new FakeApplicationInsightsConfigReader());

			var result = target.Create();

			result.NumberOfDaysToShowNonPendingRequests.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetMessageBrokerLongPolling()
		{
			var expected = RandomName.Make();
			var appConfig = new FakeConfigReader();
			appConfig.FakeSetting("MessageBrokerLongPolling", expected);
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService(),
				new FakeApplicationInsightsConfigReader());

			var result = target.Create();

			result.MessageBrokerLongPolling.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetRtaPollingInterval()
		{
			var expected = RandomName.Make();
			var appConfig = new FakeConfigReader();
			appConfig.FakeSetting("RtaPollingInterval", expected);
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService(),
				new FakeApplicationInsightsConfigReader());

			var result = target.Create();

			result.RtaPollingInterval.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetHangfireConnectionString()
		{
			var expected = RandomName.Make();
			var appConfig = new FakeConfigReader();
			appConfig.FakeConnectionString("Hangfire", expected);
			var target = new SharedSettingsFactory(appConfig, new FakeLoadPasswordPolicyService(),
				new FakeApplicationInsightsConfigReader());

			var result = target.Create();

			var decryptedResult =
				Encryption.DecryptStringFromBase64(result.Hangfire, EncryptionConstants.Image1, EncryptionConstants.Image2);
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
			var target = new SharedSettingsFactory(new FakeConfigReader(), passwordPolicyService,
				new FakeApplicationInsightsConfigReader());

			var result = target.Create();

			result.PasswordPolicy.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldGetInstrumentationKey()
		{
			var iKey = Guid.NewGuid().ToString(); 
			var appInsightConfigReader = new FakeApplicationInsightsConfigReader();
			appInsightConfigReader.SetInstrumentationKey(iKey);

			var passwordPolicyService = new FakeLoadPasswordPolicyService
			{
				DocumentAsString = RandomName.Make()
			};
			var target = new SharedSettingsFactory(new FakeConfigReader(), passwordPolicyService, appInsightConfigReader);

			var result = target.Create();
			result.InstrumentationKey.Should().Be.EqualTo(iKey);
		}
	}
}