using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.SMS;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Sms
{
	[TestFixture]
	public class SmsSenderFactoryTest
	{
		private MockRepository _mocks;
		private ISmsConfigReader _smsConfigReader;
		private SmsSenderFactory _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_smsConfigReader = _mocks.StrictMock<ISmsConfigReader>();
			_target = new SmsSenderFactory(_smsConfigReader);
		}

		[Test]
		public void ShouldReturnClickatell()
		{
			Expect.Call(_smsConfigReader.HasLoadedConfig).Return(true);
			Expect.Call(_smsConfigReader.Class).Return("Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellSmsSender");
			_mocks.ReplayAll();
			var sender = _target.Sender;
			Assert.That(sender, Is.Not.Null);
			Assert.That(sender.GetType(), Is.EqualTo(typeof(ClickatellSmsSender)));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnClickatellIfNoConfig()
		{
			Expect.Call(_smsConfigReader.HasLoadedConfig).Return(false);
			_mocks.ReplayAll();
			var sender = _target.Sender;
			Assert.That(sender, Is.Not.Null);
			Assert.That(sender.GetType(), Is.EqualTo(typeof(ClickatellSmsSender)));
			_mocks.VerifyAll();
		}
	}

}