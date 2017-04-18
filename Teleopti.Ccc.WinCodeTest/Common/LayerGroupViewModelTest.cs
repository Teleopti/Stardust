using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class LayerGroupViewModelTest
    {
        private LayerGroupViewModel _target;
        private LayerViewModelCollection _sourceCollection;
        private string _desc;
        private MockRepository _mocker;


        [SetUp]
        public void Setup()
        {
			_sourceCollection = new LayerViewModelCollection(null, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null);
            //load with some absence/mainshifts
            SchedulePartFactoryForDomain schedulePartFactoryForDomain = new SchedulePartFactoryForDomain();
            _sourceCollection.AddFromSchedulePart(schedulePartFactoryForDomain.CreateSchedulePartWithMainShiftAndAbsence());
            _desc = "Descrip";
            _target = new LayerGroupViewModel<LayerViewModel>(_desc, _sourceCollection);
        }


        [Test]
        public void VerifyDescription()
        {
            Assert.AreEqual(_desc,_target.Description);
        }
        
        [Test]
        public void VerifyCollectionViewIsBasedOnTheSourceCollection()
        {
            Assert.AreEqual(_target.Layers.SourceCollection, _sourceCollection);
        }

        [Test]
        public void VerifyFilterByTypeSpecification()
        {
            IEnumerable<AbsenceLayerViewModel> allAbsenceModels = _sourceCollection.OfType<AbsenceLayerViewModel>();
            IEnumerable<MainShiftLayerViewModel> allMainShiftModels = _sourceCollection.OfType<MainShiftLayerViewModel>();

            LayerGroupViewModel absence = new LayerGroupViewModel<AbsenceLayerViewModel>(_desc, _sourceCollection);
            LayerGroupViewModel mainShift = new LayerGroupViewModel<MainShiftLayerViewModel>(_desc, _sourceCollection);

            Assert.IsFalse(absence.Layers.IsEmpty,"No absences, test can not execute");
            Assert.IsFalse(mainShift.Layers.IsEmpty,"No mainshiftlayerviewmodels, test can not execute");
            
            foreach (MainShiftLayerViewModel model in allMainShiftModels)
            {
                Assert.IsTrue(mainShift.FilterByTypeSpecification.IsSatisfiedBy(model));
                Assert.IsFalse(absence.FilterByTypeSpecification.IsSatisfiedBy(model));
            }
           

            foreach (AbsenceLayerViewModel model in allAbsenceModels)
            {
                Assert.IsTrue(absence.FilterByTypeSpecification.IsSatisfiedBy(model));
                Assert.IsFalse(mainShift.FilterByTypeSpecification.IsSatisfiedBy(model));
         
            }
        }

        [Test]
        public void VerifyFilterByProjection()
        {
            _mocker = new MockRepository();
            ILayerViewModel mockedModel = _mocker.StrictMock<ILayerViewModel>();
            var filter = _target.FilterByProjection;

            using(_mocker.Record())
            {
                Expect.Call(mockedModel.IsProjectionLayer).Return(false);
                Expect.Call(mockedModel.IsProjectionLayer).Return(true);
            }
            using(_mocker.Playback())
            {
                Assert.IsTrue(filter.IsSatisfiedBy(mockedModel));
                Assert.IsFalse(filter.IsSatisfiedBy(mockedModel));
               
            }
        }

        [Test]
        public void VerifyThatTheLayersAreFiltered()
        {
            SchedulePartFactoryForDomain factory = new SchedulePartFactoryForDomain();
            _sourceCollection.AddFromSchedulePart(factory.AddOvertime().AddMainShiftLayer().AddMeeting().CreatePart());
            _target = new LayerGroupViewModel<LayerViewModel>(_desc, _sourceCollection);

            LayerGroupViewModel meeting = new LayerGroupViewModel<MeetingLayerViewModel>(_desc, _sourceCollection);

            Assert.IsTrue(_sourceCollection.Count(m => !(m is MeetingLayerViewModel)) > 0, "contains no other models, can not test");
           

            foreach (ILayerViewModel model in meeting.Layers)
            {
                Assert.IsFalse(model.IsProjectionLayer);
                Assert.IsTrue(model is MeetingLayerViewModel);
            }


        }

        
    }
}