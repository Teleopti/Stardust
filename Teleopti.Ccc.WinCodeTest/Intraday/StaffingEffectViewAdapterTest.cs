using System;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

using Teleopti.Wfm.Adherence.Domain.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class StaffingEffectViewAdapterTest
    {
        private StaffingEffectViewAdapter _target;
        private MockRepository _mocks;
        private IRtaStateHolder _rtaStateHolder;
        private IPerson _person;
        private DateTimePeriod _period;
        private ITeam _team;
        private IDayLayerViewModel _dayLayerViewAdapter;
        private DayLayerModel _model;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _rtaStateHolder = _mocks.DynamicMock<IRtaStateHolder>();
            _person = _mocks.DynamicMock<IPerson>();
            _period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 8, 0, 0, 0, DateTimeKind.Utc), 0);
            _team = _mocks.DynamicMock<ITeam>();

            _model = new DayLayerModel(_person, _period, _team,
										  new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()), null);
            _dayLayerViewAdapter = new DayLayerViewModel(_rtaStateHolder, null, null, null, new TestDispatcher());
            _dayLayerViewAdapter.Models.Add(_model);

            _target = new StaffingEffectViewAdapter(_dayLayerViewAdapter);
        }

        [Test]
        public void TestProperties()
        {
            Assert.AreEqual(0, _target.PositiveEffect);
            Assert.AreEqual(0, _target.NegativeEffect);
            Assert.AreEqual(0, _target.NegativeEffectPercent);
            Assert.AreEqual(0, _target.PositiveEffectPercent);
            Assert.AreEqual(0, _target.Total);
            Assert.AreEqual(0, _target.TotalPercent);
			
			_target.NegativeEffect = 1;
        	_target.Total = 1;
        	_target.PositiveEffect = 1;
        }

		[Test]
		public void VerifyCalculateEffects()
		{
			_model.StaffingEffect = 10;
			var target = new StaffingEffectViewAdapterForTest(_dayLayerViewAdapter);
			target.CallPropertyChanged(this, new PropertyChangedEventArgs("AlarmDescription"));
			_model.StaffingEffect = -10;
			target = new StaffingEffectViewAdapterForTest(_dayLayerViewAdapter);
			target.CallPropertyChanged(this, new PropertyChangedEventArgs("AlarmDescription"));
		}
		
		[Test]
		public void VerifyPropertyChangedWhenAdapterIsRemovedFromCollection()
		{
			Assert.AreEqual(1, _model.HookedEvents());
			_dayLayerViewAdapter.Models.Remove(_model);
			Assert.AreEqual(0, _model.HookedEvents());
			_dayLayerViewAdapter.Models.Add(_model);
			Assert.AreEqual(1, _model.HookedEvents());
		}
    }

	public class StaffingEffectViewAdapterForTest : StaffingEffectViewAdapter
	{
		public StaffingEffectViewAdapterForTest(IDayLayerViewModel dayLayerViewModel)
			: base(dayLayerViewModel)
		{	
		}

		public void CallPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			AdapterPropertyChanged(sender, e);
		}

	}
}
