using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class DenormalizerContextTest
	{
		private DenormalizerContext target;
		private MockRepository mocks;
		private ISendDenormalizeNotification sendDenormalizeNotification;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			sendDenormalizeNotification = mocks.DynamicMock<ISendDenormalizeNotification>();
			target = new DenormalizerContext(sendDenormalizeNotification);
		}

		[Test]
		public void ShouldSendNotificationOnDispose()
		{
			using (mocks.Record())
			{
				Expect.Call(sendDenormalizeNotification.Notify);
			}
			using (mocks.Playback())
			{
				DenormalizerContext.IsInActiveContext.Should().Be.True();
				target.Dispose();
				DenormalizerContext.IsInActiveContext.Should().Be.False();
			}
		}
	}
}