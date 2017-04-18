using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
     [TestFixture]
    public class MeetingClipboardHandlerTest
    {
         private MockRepository _mocks;
         private IMeeting _meeting;
         private MeetingClipboardHandler _target;

         [SetUp]
         public void Setup()
         {
             _mocks = new MockRepository();
             _meeting = _mocks.StrictMock<IMeeting>();
             _target = new MeetingClipboardHandler();
         }

         [Test]
         public void ShouldCloneMeetingOnGetData()
         {
             Expect.Call(_meeting.NoneEntityClone()).Return(_meeting);
             _mocks.ReplayAll();
             _target.SetData(_meeting);
             _target.GetData();
             _mocks.VerifyAll();
         }

         [Test]
         public void HasDataShouldReturnFalseAtFirst()
         {
             Assert.That(_target.HasData(), Is.False);
             _target.SetData(_meeting);
             Assert.That(_target.HasData(), Is.True);
             Assert.That(_target.GetData(), Is.Not.Null);
         }
    }
}