using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsPresenterTest
	{
		private AgentRestrictionsPresenter _presenter;
		private IAgentRestrictionsView _view;
		private IAgentRestrictionsModel _model;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_model = _mocks.StrictMock<IAgentRestrictionsModel>();
			_view = _mocks.StrictMock<IAgentRestrictionsView>();
			_presenter = new AgentRestrictionsPresenter(_view, _model);
		}

		[Test]
		public void ShouldReturnColCountEqual12()
		{
			Assert.AreEqual(12, _presenter.GridQueryColCount);
		}

		[Test]
		public void ShouldReturnRowCount()
		{
			using(_mocks.Record())
			{
				Expect.Call(_model.DisplayRows).Return(new List<AgentRestrictionsDisplayRow>());
			}

			using(_mocks.Playback())
			{
				var rows = _presenter.GridQueryRowCount;
				Assert.AreEqual(0, rows);
			}
		}
	}
}
