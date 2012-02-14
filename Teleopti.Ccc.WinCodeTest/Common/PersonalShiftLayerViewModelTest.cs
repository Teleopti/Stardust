using System;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class PersonalShiftLayerViewModelTest : LayerViewModelTest
    {
        protected override string LayerModelDescription
        {
            get { return UserTexts.Resources.PersonalShifts; }
        }

        protected override LayerViewModel CreateTestInstance(ILayer<IPayload> layer)
        {
            return new PersonalShiftLayerViewModel(layer,null);
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
