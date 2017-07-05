using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class SharedSettingsTest
	{
		[Test]
		public void ShouldLeaveOrgAsIs()
		{
			var key = RandomName.Make();
			var value = RandomName.Make();
			var org = new Dictionary<string, string>{{key, value}};
			var target = new SharedSettings{MessageBroker = RandomName.Make()};
			var result = target.AddToAppSettings(org);
			result[key].Should().Be.EqualTo(value);
		}

		[Test]
		public void ShouldLeaveSharedSettingAsIs()
		{
			var value = RandomName.Make();
			var org = new Dictionary<string, string> { { RandomName.Make(), RandomName.Make() } };
			var target = new SharedSettings { MessageBroker = value };
			var result = target.AddToAppSettings(org);
			result["MessageBroker"].Should().Be.EqualTo(value);
		}

		[Test]
		public void NumberOfDaysToShowNonPendingRequestsCanBeOverriden()
		{
			const string value = "47";
			var org = new Dictionary<string, string> { { "NumberOfDaysToShowNonPendingRequests", value } };
			var target = new SharedSettings { NumberOfDaysToShowNonPendingRequests = 53 };
			var result = target.AddToAppSettings(org);
			result["NumberOfDaysToShowNonPendingRequests"].Should().Be.EqualTo("47");
		}

		[Test]
		public void MessageBrokerCanBeOverriden()
		{
			var value = RandomName.Make();
			var org = new Dictionary<string, string> { { "MessageBroker", value } };
			var target = new SharedSettings { MessageBroker = RandomName.Make() };
			var result = target.AddToAppSettings(org);
			result["MessageBroker"].Should().Be.EqualTo(value);
		}

		[Test]
		public void MessageBrokerLongPollingCanBeOverriden()
		{
			var value = RandomName.Make();
			var org = new Dictionary<string, string> { { "MessageBrokerLongPolling", value } };
			var target = new SharedSettings { MessageBrokerLongPolling = RandomName.Make() };
			var result = target.AddToAppSettings(org);
			result["MessageBrokerLongPolling"].Should().Be.EqualTo(value);
		}

		[Test]
		public void RtaPollingIntervalCanBeOverriden()
		{
			var value = RandomName.Make();
			var org = new Dictionary<string, string> { { "RtaPollingInterval", value } };
			var target = new SharedSettings { RtaPollingInterval = RandomName.Make() };
			var result = target.AddToAppSettings(org);
			result["RtaPollingInterval"].Should().Be.EqualTo(value);
		}
	}
}