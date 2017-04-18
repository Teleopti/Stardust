using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging.Filters;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging.Filter
{
    [TestFixture]
    public class PushMessageFilterTest
    {
      
        private MockRepository _mocker;
        private IFollowUpPushMessageViewModel _model;

        [SetUp]
        public void Setup()
        {
            
            _mocker = new MockRepository();
            _model = _mocker.StrictMock<IFollowUpPushMessageViewModel>();
           
        }
        #region specification

        [Test]
        public void VerifyPushMessageIsRepliedSpecification()
        {
            FollowUpDialogueForTest dialogue1 = new FollowUpDialogueForTest();
            FollowUpDialogueForTest dialogue2 = new FollowUpDialogueForTest();
            PushMessageIsRepliedSpecification specification =new PushMessageIsRepliedSpecification();
            ObservableCollection<IFollowUpMessageDialogueViewModel> dialogues = new ObservableCollection<IFollowUpMessageDialogueViewModel>() { dialogue1, dialogue2};

            dialogue1.IsReplied = true;
            dialogue2.IsReplied = false;

            using(_mocker.Record())
            {
                Expect.Call(_model.Dialogues).Return(dialogues).Repeat.Twice();
            }
            using(_mocker.Playback())
            {
                Assert.IsFalse(specification.IsSatisfiedBy(_model));
                dialogue2.IsReplied = true;
                Assert.IsTrue(specification.IsSatisfiedBy(_model));

            }

        }
        #endregion
    }

    
}
