using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Outbound
{

	[TestFixture]
	public class CampaignTest
	{
        private IOutboundCampaign _target;

		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void x()
		{
			_target = new Campaign();
			_target.CallListLen = 2400;
			_target.TargetRate = 100;
			_target.RightPartyConnectRate = 100;
			_target.RightPartyAverageHandlingTime = 600;
			_target.ConnectRate = 100;
			Assert.AreEqual(2400, _target.CampaignTasks());
			Assert.AreEqual(TimeSpan.FromSeconds(600), _target.AverageTaskHandlingTime());
		}
	}
}
