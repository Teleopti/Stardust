using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;

using Teleopti.Wfm.Adherence.Domain.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class AgentStateViewAdapterTest
    {
        private AgentStateViewAdapter _target;
        private MockRepository _mocks;
        private IRtaStateGroup _rtaStateGroup;
        private IDayLayerViewModel _dayLayerViewModel;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _rtaStateGroup = _mocks.DynamicMock<IRtaStateGroup>();
            _dayLayerViewModel = _mocks.DynamicMock<IDayLayerViewModel>();

            _target = new AgentStateViewAdapter(_rtaStateGroup, _dayLayerViewModel);
        }

        [Test]
        public void VerifyProperties()
        {
            _mocks.ReplayAll();

            Assert.IsNotNull(_target);
            Assert.AreEqual(0, _target.TotalPersons);
            Assert.AreEqual(_rtaStateGroup, _target.StateGroup);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanRefreshAgentState()
        {
        	var wasCalled = false;
        	_target.PropertyChanged += (s, e) => wasCalled = true;
            var dayLayerModel =
                new DayLayerModel(
                    new Person(),
                    new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
                                       new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
                    new Team(),
                    new LayerViewModelCollection(
                        new EventAggregator(),
						new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()),
                    new CommonNameDescriptionSetting("test"));

            var collection = new Collection<DayLayerModel> { dayLayerModel };

        	_dayLayerViewModel.Expect(d => d.Models).Return(collection);
            _mocks.ReplayAll();

            _target.Refresh();
            _mocks.VerifyAll();			
            Assert.That(_target.TotalPersons, Is.EqualTo(1));
			Assert.That(wasCalled, Is.True);
        }

		[Test]
		public void ShouldAddToTotalPersonsWhenStateGroupNotDefined()
		{
			var dayLayerModel =
				new DayLayerModel(
					new Person(),
					new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
					                   new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
					new Team(),
					new LayerViewModelCollection(
						new EventAggregator(),
						new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()),
					new CommonNameDescriptionSetting("test"))
					{
						CurrentStateDescription = null
					};

			var collection = new Collection<DayLayerModel> { dayLayerModel };

			_dayLayerViewModel.Expect(d => d.Models).Return(collection);
			_rtaStateGroup.Expect(r => r.Name).Return("Not defined");
			_mocks.ReplayAll();

			_target.Refresh();
			_mocks.VerifyAll();
			Assert.That(_target.TotalPersons, Is.EqualTo(1));

		}

		[Test]
		public void ShouldAddToNotLoggedOutWhenNotDefined()
		{
			_rtaStateGroup = new RtaStateGroup("OLAANDASADSSECRETNAMETWO", false, false);
			
			IList<IRtaStateGroup> rtaStateGroups = new List<IRtaStateGroup>()
			{
				_rtaStateGroup, new RtaStateGroup("LoggedOut",false,false) {IsLogOutState = true},
				new RtaStateGroup("InCall", false, false)
			};
			_target = new AgentStateViewAdapter(_rtaStateGroup, _dayLayerViewModel,rtaStateGroups);
			var dayLayerModel =
				new DayLayerModel(
					new Person(),
					new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
											 new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
					new Team(),
					new LayerViewModelCollection(
						new EventAggregator(),
						new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()),
					new CommonNameDescriptionSetting("test"))
				{
					CurrentStateDescription = null
				};

			var dayLayerModel2 =
				new DayLayerModel(
					new Person(),
					new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
											 new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
					new Team(),
					new LayerViewModelCollection(
						new EventAggregator(),
						new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()),
					new CommonNameDescriptionSetting("test"))
				{
					CurrentStateDescription = "InCall"
				};

			var dayLayerModel3 =
				new DayLayerModel(
					new Person(),
					new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
											 new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
					new Team(),
					new LayerViewModelCollection(
						new EventAggregator(),
						new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()),
					new CommonNameDescriptionSetting("test"))
				{
					CurrentStateDescription = "LoggedOut"
				};

			var collection = new Collection<DayLayerModel> { dayLayerModel, dayLayerModel2,dayLayerModel3 };

			_dayLayerViewModel.Expect(d => d.Models).Return(collection);

			_mocks.ReplayAll();

			_target.Refresh();
			_mocks.VerifyAll();
			Assert.That(_target.TotalPersons, Is.EqualTo(2));

		}

		[Test]
		public void ShouldAddToLoggedOnWhenAgentIsAvailable()
		{
			_rtaStateGroup = new RtaStateGroup("OLAANDASADSSECRETNAME", false, false);

			IList<IRtaStateGroup> rtaStateGroups = new List<IRtaStateGroup>()
			{
				_rtaStateGroup, new RtaStateGroup("LoggedOut", false, false) { IsLogOutState = true },
				new RtaStateGroup("InCall", false, false) 
			};
			_target = new AgentStateViewAdapter(_rtaStateGroup, _dayLayerViewModel, rtaStateGroups);
			var dayLayerModel =
				new DayLayerModel(
					new Person(),
					new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
											 new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
					new Team(),
					new LayerViewModelCollection(
						new EventAggregator(),
						new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()),
					new CommonNameDescriptionSetting("test"))
				{
					CurrentStateDescription = null
				};

			var dayLayerModel2 =
				new DayLayerModel(
					new Person(),
					new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
											 new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
					new Team(),
					new LayerViewModelCollection(
						new EventAggregator(),
						new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()),
					new CommonNameDescriptionSetting("test"))
				{
					CurrentStateDescription = "InCall"
				};

			var dayLayerModel3 =
				new DayLayerModel(
					new Person(),
					new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
											 new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
					new Team(),
					new LayerViewModelCollection(
						new EventAggregator(),
						new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()),
					new CommonNameDescriptionSetting("test"))
				{
					CurrentStateDescription = "LoggedOut"
				};

			var collection = new Collection<DayLayerModel> { dayLayerModel, dayLayerModel2, dayLayerModel3 };

			_dayLayerViewModel.Expect(d => d.Models).Return(collection);

			_mocks.ReplayAll();

			_target.Refresh();
			_mocks.VerifyAll();
			Assert.That(_target.TotalPersons, Is.EqualTo(1));

		}
	}
}