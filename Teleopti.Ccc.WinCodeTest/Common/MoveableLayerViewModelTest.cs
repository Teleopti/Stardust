using System.Linq;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class MoveableLayerViewModelTest : LayerViewModelTest
    {
    	private readonly TesterForCommandModels _models = new TesterForCommandModels();
        private readonly MockRepository _mocks = new MockRepository();
        private readonly IEventAggregator _eventAggregator=new EventAggregator();

        protected override string LayerModelDescription
        {
            get { return UserTexts.Resources.Activity; }
        }

        protected override LayerViewModel CreateTestInstance(ILayer layer)
        {
            return new MainShiftLayerViewModel(layer, null);
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
        public void VerifyMoveUpDownCommand()
        {
            //Check taht the layer actually moves and that the observer is called (for resorting etc.)

            #region setup
            ILayerViewModelObserver observer = _mocks.StrictMock<ILayerViewModelObserver>();
            MainShift shift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
            ILayer<IActivity> firstLayer =
                (from l in shift.LayerCollection
                 orderby l.OrderIndex
                 select l).First();
            MainShiftLayerViewModel model = new MainShiftLayerViewModel(observer, firstLayer, shift, _eventAggregator);
            #endregion

            #region expectations
            using (_mocks.Record())
            {
                Expect.Call(()=>observer.LayerMovedVertically(model)).Repeat.Twice();
            }
            #endregion
            //Check texts
            Assert.AreEqual(model.MoveUpCommand.Text, UserTexts.Resources.MoveUp);
            Assert.AreEqual(model.MoveDownCommand.Text, UserTexts.Resources.MoveDown);

            using(_mocks.Playback())
            {
                //Execute MoveDown
                _models.ExecuteCommandModel(model.MoveDownCommand);
                Assert.IsTrue(model.Layer.OrderIndex == 1, "Orderindex should have changed to 1");

                //Execute MoveUp
                _models.ExecuteCommandModel(model.MoveUpCommand);
                Assert.IsTrue(model.Layer.OrderIndex == 0, "Orderindex should have changed back to 0");
            }
        }

        [Test]
        public void VerifyCanMoveUpDownCommand()
        {
            #region setup
            MainShift shift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();

            IOrderedEnumerable<ILayer<IActivity>> layers =
                from l in shift.LayerCollection
                orderby l.OrderIndex
                select l;

            MainShiftLayerViewModel highest = new MainShiftLayerViewModel(layers.First(), shift, _eventAggregator);
            MainShiftLayerViewModel lowest = new MainShiftLayerViewModel(layers.Last(), shift, _eventAggregator);
            #endregion

            Assert.IsTrue(_models.CanExecute(highest.MoveDownCommand), "Can move down because there are other layers below");
            Assert.IsTrue(_models.CanExecute(lowest.MoveUpCommand), "Can  move up because there are other layers that are higher");

            Assert.IsFalse(_models.CanExecute(highest.MoveUpCommand), "Cannot move up layer because its the highest in the shift");
            Assert.IsFalse(_models.CanExecute(lowest.MoveDownCommand), "Cannot move down layer because its the lowest in the shift");
        }

        [Test]
        public void VerifyCannotMoveUpOrDownIfNoShift()
        {
            MainShiftLayerViewModel modelWithoutParent = new MainShiftLayerViewModel(new MainShiftActivityLayer(ActivityFactory.CreateActivity("test"), Period), null);
            TesterForCommandModels testerForCommandModels = new TesterForCommandModels();
            Assert.IsNull(modelWithoutParent.Parent);
            Assert.IsFalse(testerForCommandModels.CanExecute(modelWithoutParent.MoveUpCommand));
            Assert.IsFalse(testerForCommandModels.CanExecute(modelWithoutParent.MoveDownCommand));
        }

        [Test]
        public void VerifyDeleteCommandCallsObserver()
        {
            #region setup
            ILayerViewModelObserver observer = _mocks.StrictMock<ILayerViewModelObserver>();
         

            MainShift shift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
            ILayer<IActivity> firstLayer =
                (from l in shift.LayerCollection
                 orderby l.OrderIndex
                 select l).First();
            MainShiftLayerViewModel model = new MainShiftLayerViewModel(observer, firstLayer, shift, _eventAggregator);
            

            #endregion
            #region expectations
            using (_mocks.Record())
            {
                Expect.Call(()=>observer.RemoveActivity(model));
            }
            #endregion
         
            using (_mocks.Playback())
            {
                //Execute Delete
                _models.ExecuteCommandModel(model.DeleteCommand);
            }
        }



    }
}