using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class OvertimeLayerViewModelTest : LayerViewModelTest
    {

        protected override string LayerModelDescription
        {
            get { return UserTexts.Resources.Overtime; }
        }

        protected override LayerViewModel CreateTestInstance(ILayer layer)
        {
            return new OvertimeLayerViewModel(layer);
        }

        protected override bool ExpectMovePermitted
        {
            get { return true; }
        }

        protected override bool ExpectIsPayloadChangePermitted
        {
            get { return false;}
        }

        protected override bool Opaque
        {
            get { return true; }
        }

        [Test]
        public void VerifyCanGetDescriptionFromMultiplicatorSet()
        {
            IActivity activity = Mocks.StrictMock<IActivity>();
            DateTimePeriod period = new DateTimePeriod();
            IMultiplicatorDefinitionSet multiplicatorDefinitionSet = Mocks.StrictMock<IMultiplicatorDefinitionSet>();
            OvertimeShiftActivityLayer overtimeLayer = new OvertimeShiftActivityLayer(activity,period,multiplicatorDefinitionSet);

            Expect.Call(multiplicatorDefinitionSet.Name).Return("Qualified overtime");

            Mocks.ReplayAll();

            var target = new OvertimeLayerViewModel(overtimeLayer);
            Assert.AreEqual("Qualified overtime",target.LayerDescription);
        }
    }
}
