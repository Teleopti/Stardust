using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
	 [TestFixture]
	public class FetchMeetingForCurrentUserChangeCommandTest
	{
	 	private MockRepository _mocks;
	 	private IMeetingOverviewView _meetingOverView;
		private IMeetingOverviewViewModel _model;
	 	private FetchMeetingForCurrentUserChangeCommand _target;

	 	[SetUp]
		 public void Setup()
		 {
			 _mocks = new MockRepository();
	 		_meetingOverView = _mocks.StrictMock<IMeetingOverviewView>();
			_model = _mocks.StrictMock<IMeetingOverviewViewModel>();
			_target = new FetchMeetingForCurrentUserChangeCommand(_meetingOverView, _model);
		 }

		[Test]
		public void ShouldAlwaysReturnTrueOnCanExecute()
		{
			Assert.That(_target.CanExecute(), Is.True);
		}

		[Test]
		public void ShouldGetFetchModeFromViewAndSetOnModel()
		{
			Expect.Call(_meetingOverView.FetchForCurrentUser).Return(true);
			Expect.Call(() => _model.IncludeForOrganizer = true);
			Expect.Call(() => _meetingOverView.ReloadMeetings());
			_mocks.ReplayAll();
			_target.Execute();
			_mocks.VerifyAll();
		}
	}

}