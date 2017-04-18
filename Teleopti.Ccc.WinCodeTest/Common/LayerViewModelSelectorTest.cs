using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class LayerViewModelSelectorTest
    {
        private MockRepository _mocks;
        private ILayerViewModelObserver _layerViewModelObserver;
        private IList<ILayerViewModel> _models;
        private IScheduleDay _scheduleDay;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _layerViewModelObserver = _mocks.StrictMock<ILayerViewModelObserver>();
            _scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShiftWithDifferentActivities();
            _models = new CreateLayerViewModelService().CreateViewModelsFromSchedule(_scheduleDay, null, TimeSpan.FromMinutes(2), _layerViewModelObserver);



        }

        [Test]
        public void VerifySelectedLayer()
        {
            var target = new LayerViewModelSelector(_models[2]);
            Assert.AreEqual(target.SelectedLayer, _models[2]);
        }

        [Test]
        public void VerifyDoesNotSelectIfNotInTheList()
        {
            ILayerViewModelSelector target = new LayerViewModelSelector(_models[3]);

            using (_mocks.Record())
            {
                Expect.Call(() => _layerViewModelObserver.SelectLayer(null)).Repeat.Never();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(target.TryToSelectLayer(new List<ILayerViewModel>(), _layerViewModelObserver));
            }
        }

        [Test]
        public void VerifyThatLayerWithSamePeriodAndActivityGetsSelected()
        {
            //create another list, because it could be a EQUAL ScheduleDay, but not the same
            IList<ILayerViewModel> anotherListOfEqualModels = new CreateLayerViewModelService().CreateViewModelsFromSchedule(_scheduleDay, null, TimeSpan.FromMinutes(2), _layerViewModelObserver);

            ILayerViewModelSelector target = new LayerViewModelSelector(_models[3]);

            using (_mocks.Record())
            {
                Expect.Call(() => _layerViewModelObserver.SelectLayer(anotherListOfEqualModels[3])); //It will try to select the third layer (because it matches the targets layer)
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(target.TryToSelectLayer(anotherListOfEqualModels, _layerViewModelObserver));

            }
        }

        [Test]
        public void VerifyChecksIfTheScheduleDayAffectsTheSameDateAndPerson()
        {
            //this could (and is) tested separate, but it was easier/cleaner to test here than injecting the specification.

            IScheduleDay scheduleDayWithOtherPerson =
                new SchedulePartFactoryForDomain(PersonFactory.CreatePerson("anotherPerson"), _scheduleDay.Scenario,
                                                 _scheduleDay.Period, SkillFactory.CreateSkill("someSkill")).CreatePart();

            IScheduleDay scheduleDayWithOtherPeriod =
                new SchedulePartFactoryForDomain(PersonFactory.CreatePerson("anotherPerson"), _scheduleDay.Scenario,
                                                 _scheduleDay.Period.MovePeriod(TimeSpan.FromDays(2)), SkillFactory.CreateSkill("someSkill")).CreatePart();


            
            var target = new LayerViewModelSelector(_models[3]);
            Assert.IsTrue(target.ScheduleAffectsSameDayAndPerson(_scheduleDay));
           
            Assert.IsFalse(target.ScheduleAffectsSameDayAndPerson(scheduleDayWithOtherPerson));
            Assert.IsFalse(target.ScheduleAffectsSameDayAndPerson(scheduleDayWithOtherPeriod));

        }





    }
}