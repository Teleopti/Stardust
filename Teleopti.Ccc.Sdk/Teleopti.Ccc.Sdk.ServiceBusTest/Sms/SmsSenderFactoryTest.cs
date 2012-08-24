using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.SMS;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Sms
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), TestFixture]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), Test]
		public void ShouldReturnClickatell()
		{
			Expect.Call(_smsConfigReader.HasLoadedConfig).Return(true);
			Expect.Call(_smsConfigReader.ClassName).Return("Teleopti.Ccc.Sdk.ServiceBus.SMS.ClickatellSmsSender");
			_mocks.ReplayAll();
			var sender = _target.Sender;
			Assert.That(sender, Is.Not.Null);
			Assert.That(sender.GetType(), Is.EqualTo(typeof(ClickatellSmsSender)));
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), Test]
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