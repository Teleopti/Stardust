using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;

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
            var dayLayerModel =
                new DayLayerModel(
                    new Person(),
                    new DateTimePeriod(new DateTime(2012, 11, 09, 20, 20, 00, DateTimeKind.Utc),
                                       new DateTime(2012, 11, 10, 20, 20, 00, DateTimeKind.Utc)),
                    new Team(),
                    new LayerViewModelCollection(
                        new EventAggregator(),
                        new CreateLayerViewModelService()),
                    new CommonNameDescriptionSetting("test"));

            var collection = new Collection<DayLayerModel> { dayLayerModel };

            Expect.Call(_dayLayerViewModel.Models).Return(collection);
            _mocks.ReplayAll();

            _target.Refresh();
            _mocks.VerifyAll();
            Assert.AreEqual(1, _target.TotalPersons);
        }
    }
}
