using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging.Filters;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging.Filter
{
    /// <summary>
    /// Tests for testing filtering FollowUpDialogueViewModels and specifications.....
    /// </summary>
    [TestFixture]
    public class MessageDialogueFilterTest
    {
        private SpecificationFilter<IFollowUpMessageDialogueViewModel> _target;
        private FollowUpDialogueForTest _model1;
        private FollowUpDialogueForTest _model2;
        private string _reply1;
        private string _reply2;

        [SetUp]
        public void Setup()
        {
            _reply1 = "Yes";
            _reply2 = "No";
            _target = new SpecificationFilter<IFollowUpMessageDialogueViewModel>();
            _model1 = new FollowUpDialogueForTest() { Reply = _reply1 };
            _model2 = new FollowUpDialogueForTest() { Reply = _reply2 };
        }

        [Test]
        public void VerifyDialogueIsRepliedSpecification()
        {
            DialogueIsRepliedSpecification specification = new DialogueIsRepliedSpecification();

            _model1.IsReplied = true;
            _model2.IsReplied = false;

            Assert.IsTrue(specification.IsSatisfiedBy(_model2));
            Assert.IsFalse(specification.IsSatisfiedBy(_model1));
        }

        [Test]
        public void VerifyReplySpecification()
        {
            DialogueReplySpecification specification = new DialogueReplySpecification(_reply1);
            Assert.IsTrue(specification.IsSatisfiedBy(_model1));
            Assert.IsFalse(specification.IsSatisfiedBy(_model2));
        }

        [Test]
        public void VerifyParameter()
        {
            Assert.Throws<ArgumentException>(() => _target.FilterOutSpecification("test"));
        }

        [Test]
        public void VerifyParameterAllButSpecification()
        {
			Assert.Throws<ArgumentException>(() => _target.FilterAllButSpecification("test"));
        }

        [Test]
        public void VerifyCanCombineFilters()
        {
            _model1.IsReplied = true;
            _model2.IsReplied = false;

            //Filter is null, should return true
            Assert.IsTrue(_target.FilterOutSpecification(_model1), "no filters show both");
            Assert.IsTrue(_target.FilterOutSpecification(_model2), "no filters show both");

            _target.Filter = new DialogueIsRepliedSpecification();
            Assert.IsTrue(_target.FilterOutSpecification(_model1), "model is filtered out because its replied");
            Assert.IsFalse(_target.FilterOutSpecification(_model2));

            _target.Filter = new DialogueReplySpecification(_reply2).Or(new DialogueIsRepliedSpecification());
            Assert.IsTrue(_target.FilterOutSpecification(_model1));
            Assert.IsFalse(_target.FilterOutSpecification(_model2), "model is filteredout because reply is reply2 (but its not replied, but its just a mock so I dont care)");
        }

        [Test]
        public void VerifyFilterAllButSpecification()
        {
            _model1.IsReplied = true;
            _model2.IsReplied = false;

            //Filter is null, should return true
            Assert.IsFalse(_target.FilterAllButSpecification(_model1), "no specification show nothing");
            Assert.IsFalse(_target.FilterAllButSpecification(_model2), "no specification show nothing");

            _target.Filter = new DialogueIsRepliedSpecification();
            Assert.IsTrue(_target.FilterAllButSpecification(_model2), "only model2 is satisfied by specification");
            Assert.IsFalse(_target.FilterAllButSpecification(_model1));
        }
    }
}
