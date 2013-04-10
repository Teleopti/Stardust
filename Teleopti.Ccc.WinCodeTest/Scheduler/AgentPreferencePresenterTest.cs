using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
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
		private IPreferenceDay _preferenceDay;
		private IPerson _person;
		private DateOnly _dateOnly;
		private IPreferenceRestriction _preferenceRestriction;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IAgentPreferenceView>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_presenter = new AgentPreferencePresenter(_view, _scheduleDay);
			_person = new Person();
			_dateOnly = new DateOnly(2013,1,1);
			_preferenceRestriction = new PreferenceRestriction();
			_preferenceDay = new PreferenceDay(_person, _dateOnly, _preferenceRestriction);

		}

		[Test]
		public void ShouldInitializePresenter()
		{
			Assert.AreEqual(_view, _presenter.View);
			Assert.AreEqual(_scheduleDay, _presenter.ScheduleDay);
		}

		[Test]
		public void ShouldUpdateView()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(() => _view.Update(_preferenceRestriction));
			}

			using (_mock.Playback())
			{
				_presenter.UpdateView();
			}
		}
	}
}
