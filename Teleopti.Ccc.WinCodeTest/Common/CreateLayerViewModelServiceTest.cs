using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class CreateLayerViewModelServiceTest
    {
        private MockRepository _mocks;
        private ILayerViewModelObserver _layerViewModelObserver;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _layerViewModelObserver = _mocks.DynamicMock<ILayerViewModelObserver>();
        }

        [Test]
        public void VerifyOnlyChecksIfItShouldSelectLayerIfSelectedLayerHasTheSamePersonAndDateAsTheNewSchedule()
        {
            ILayerViewModelSelector selector = _mocks.StrictMock<ILayerViewModelSelector>();
            ICreateLayerViewModelService target = new CreateLayerViewModelService();
            IScheduleDay scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();
            using (_mocks.Record())
            {
                Expect.Call(selector.ScheduleAffectsSameDayAndPerson(scheduleDay)).Return(false);
                Expect.Call(() => selector.TryToSelectLayer(null, null)).Repeat.Never();
            }
            using (_mocks.Playback())
            {
                target.CreateViewModelsFromSchedule(selector, scheduleDay, null, TimeSpan.FromMinutes(1), null);
            }
        }

        [Test]
        public void VerifyTriesToSelectLayerViewModelIfTheScheduleDayHasTheSameHasTheSamePersonAndDateAsTheOldSelectedLayer()
        {
            ILayerViewModelSelector selector = _mocks.StrictMock<ILayerViewModelSelector>();
            ICreateLayerViewModelService target = new CreateLayerViewModelService();
            IScheduleDay scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();


            using (_mocks.Record())
            {
                Expect.Call(selector.ScheduleAffectsSameDayAndPerson(scheduleDay)).Return(true); //Verify that it compares the schedule
                Expect.Call(selector.TryToSelectLayer(null, null)).IgnoreArguments().Return(true); //Verify that it tries to select if schedulecheck returns true
                LastCall.Constraints(Is.Anything(), Is.Equal(_layerViewModelObserver));     //Verify that it passes the observer (so the ILayerViewModelSelector can call setlayer)
            }
            using (_mocks.Playback())
            {
                target.CreateViewModelsFromSchedule(selector, scheduleDay, null, TimeSpan.FromMinutes(1), _layerViewModelObserver);
            }
        }

    }
}
