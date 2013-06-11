using System.Linq;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class AbsenceLayerViewModelTest : LayerViewModelTest
    {
        private SchedulePartFactoryForDomain _factory = new SchedulePartFactoryForDomain();
        private MockRepository _mocks = new MockRepository();
        private TesterForCommandModels _models = new TesterForCommandModels();
        private IEventAggregator _eventAggregator = new EventAggregator();

         
        protected override string LayerModelDescription
        {
            get { return UserTexts.Resources.Absence; }
        }

        protected override LayerViewModel CreateTestInstance(ILayer layer)
        {
            return AbsenceLayerViewModel.CreateForProjection(layer);
        }

        protected override bool ExpectMovePermitted
        {
            get { return true; }
        }

        protected override bool ExpectIsPayloadChangePermitted
        {
            get { return true; }
        }

        [Test]
        public void VerifyCannotMoveAbsenceLayerVertical()
        {
            #region setup
            LayerViewModelCollection collection = new LayerViewModelCollection(_eventAggregator,new CreateLayerViewModelService());
            IScheduleDay part = _factory.CreateSchedulePartWithMainShiftAndAbsence();
            collection.AddFromSchedulePart(part);
            #endregion

            //Get the first absencelayer:
            ILayerViewModel model = collection.First(l => l is AbsenceLayerViewModel);
            Assert.IsFalse(model.CanMoveUp);
            Assert.IsFalse(model.CanMoveDown);
        }
        [Test]
        public void VerifyDeleteCommandCallsObserver()
        {
            #region setup
            ILayerViewModelObserver observer = _mocks.StrictMock<ILayerViewModelObserver>();
         
            IScheduleDay part = _factory.CreateSchedulePartWithMainShiftAndAbsence();
            var absenceLayer = part.PersonAbsenceCollection().First().Layer;
            AbsenceLayerViewModel model = AbsenceLayerViewModel.CreateNormal(observer, absenceLayer, _eventAggregator);

            #endregion
            #region expectations
            using (_mocks.Record())
            {
                Expect.Call(() => observer.RemoveAbsence(model));            
               
            }
            #endregion

            using (_mocks.Playback())
            {
               
                _models.ExecuteCommandModel(model.DeleteCommand);
            }
        }

    }
}
