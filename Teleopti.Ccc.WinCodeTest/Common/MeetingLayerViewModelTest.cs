using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class MeetingLayerViewModelTest : LayerViewModelTest
    {
        protected override string LayerModelDescription
        {
            get { return UserTexts.Resources.Meeting; }
        }

        protected override LayerViewModel CreateTestInstance(ILayer layer)
        {
            return new MeetingLayerViewModel(layer,null);
        }

        protected override bool ExpectMovePermitted
        {
            get { return false; }
        }

        protected override bool ExpectIsPayloadChangePermitted
        {
            get { return false; }
        }

        [Test]
        public void VerifyCanMoveUpDoesNotCheckShiftWhenMoveNotPermitted()
        {
            ILayer meetingLayer = new AbsenceLayer(AbsenceFactory.CreateAbsence("test"), Period);
            MockRepository mocks = new MockRepository();
            IShift shift = mocks.StrictMock<IShift>();
            MeetingLayerViewModel model = new MeetingLayerViewModel(meetingLayer,null);
            using (mocks.Record())
            {
                Expect.Call(shift.LayerCollection).Repeat.Never();
            }
            using (mocks.Playback())
            {
                Assert.IsFalse(model.CanMoveUp, "There is a Collection, but it should never get called since moving meeting is not permitted");
                Assert.IsFalse(model.CanMoveDown, "There is a Collection, but it should never get called since moving meeting is not permitted");
            }
        }

    }
}
