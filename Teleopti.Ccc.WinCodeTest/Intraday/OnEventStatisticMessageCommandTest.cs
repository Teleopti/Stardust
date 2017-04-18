using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class OnEventStatisticMessageCommandTest
	{
		private MockRepository mocks;
		private IIntradayView _view;
		private OnEventStatisticMessageCommand target;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_view = mocks.StrictMock<IIntradayView>();
			target = new OnEventStatisticMessageCommand(_view);
		}

		[Test]
		public void ShouldRefreshView()
		{
			using (mocks.Record())
			{
				_view.DrawSkillGrid();
			}
			using (mocks.Playback())
			{
				target.Execute(new EventMessage());
			}
		}
	}
}