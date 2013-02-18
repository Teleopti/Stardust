using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class MainShiftLayerViewModelTest : LayerViewModelTest
    {

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

    }
}
