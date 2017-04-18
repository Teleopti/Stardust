using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging.Filters;
using Teleopti.Ccc.WinCodeTest.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging
{
    [TestFixture]
    public class ReplyOptionViewModelTest
    {
        private ReplyOptionViewModel _target;
        private ObservableCollection<IFollowUpMessageDialogueViewModel> _models;
        private MockRepository _mocker;
        private string _reply;
        private FollowUpDialogueForTest _dialogue1;
        private FollowUpDialogueForTest _dialogue2;
        private TesterForCommandModels _testerForCommandModels;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _reply = "reply";
            _dialogue1 = new FollowUpDialogueForTest() {Reply = _reply};
            _dialogue2 = new FollowUpDialogueForTest();
            _models = new ObservableCollection<IFollowUpMessageDialogueViewModel>();
            _models.Add(_dialogue1);
            _models.Add(_dialogue2);
            _testerForCommandModels = new TesterForCommandModels();
            _target = new ReplyOptionViewModel(_reply,_models);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_reply,_target.Reply);
            Assert.IsFalse(_target.IsNotRepliedOption);
        }

        [Test]
        public void VerifyCreatesAFilterBasedOnReply()
        {
            DialogueReplySpecification specification = _target.Filter.Filter as DialogueReplySpecification;
            Assert.AreEqual(_reply,specification.Reply);
        }

        [Test]
        public void VerifyCollectionIsFilteredOnAFilteredView()
        {
            Assert.IsTrue(_target.FilteredView.Contains(_dialogue1));
            Assert.IsFalse(_target.FilteredView.Contains(_dialogue2));
            Assert.IsTrue(_target.DefaultView.Contains(_dialogue1));
            Assert.IsTrue(_target.DefaultView.Contains(_dialogue2));
        }

        [Test]
        public void VerifyFilterCommand()
        {
            IFilterTarget filterTarget  = _mocker.StrictMock<IFilterTarget>();

            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.FilterCommand));
            _target.FilterTarget = filterTarget;
            Assert.IsTrue(_testerForCommandModels.CanExecute(_target.FilterCommand));

            using(_mocker.Record())
            {
                Expect.Call(() => filterTarget.AddFilter(_target));
                Expect.Call(() => filterTarget.RemoveFilter(_target));
            }
            using(_mocker.Playback())
            {
                _testerForCommandModels.ExecuteCommandModel(_target.FilterCommand);
                _target.FilterIsActive = true;
                _testerForCommandModels.ExecuteCommandModel(_target.FilterCommand);
            }
        }

        [Test]
        public void VerifyCanCreateNotRepliedReplyOption()
        {
            _target = new ReplyOptionViewModel(_models);
            Assert.AreEqual(_target.Reply,UserTexts.Resources.NotReplied);
            Assert.IsTrue(_target.Filter.Filter is DialogueIsRepliedSpecification);
            Assert.IsTrue(_target.IsNotRepliedOption);
        }
    }
}
