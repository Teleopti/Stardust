using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferencePresenterTest
	{
		private AgentPreferencePresenter _presenter;
		private IAgentPreferenceView _view;
		private MockRepository _mock;
		private IScheduleDay _scheduleDay;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IAgentPreferenceView>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_presenter = new AgentPreferencePresenter(_view, _scheduleDay);	
		}

		[Test]
		public void ShouldInitializePresenter()
		{
			Assert.AreEqual(_view, _presenter.View);
			Assert.AreEqual(_scheduleDay, _presenter.ScheduleDay);
		}
	}
}
