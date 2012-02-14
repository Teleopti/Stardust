using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class ShiftTradeSkillSpecificationTest
    {
        private IShiftTradeSkillSpecification _target;
        private MockRepository _mock;

        private IPerson _personFrom;
        private IPerson _personTo;
        private IWorkflowControlSet _workflowControlSetFrom;
        private IWorkflowControlSet _workflowControlSetTo;
        private IPersonPeriod _periodFrom;
        private IPersonPeriod _periodTo;

        private ISkill _skill1;
        private ISkill _skill2;
        private ISkill _skill3;

        private IPersonSkill _personSkill1;
        private IPersonSkill _personSkill2;
        private IPersonSkill _personSkill3;

        [SetUp]
        public void Setup()
        {
            _target = new ShiftTradeSkillSpecification();
            _mock = new MockRepository();
            _personFrom = _mock.StrictMock<IPerson>();
            _personTo = _mock.StrictMock<IPerson>();
            _workflowControlSetFrom = _mock.StrictMock<IWorkflowControlSet>();
            _workflowControlSetTo = _mock.StrictMock<IWorkflowControlSet>();
            _periodFrom = _mock.StrictMock<IPersonPeriod>();
            _periodTo = _mock.StrictMock<IPersonPeriod>();

            _skill1 = _mock.StrictMock<ISkill>();
            _skill2 = _mock.StrictMock<ISkill>();
            _skill3 = _mock.StrictMock<ISkill>();

            _personSkill1 = new PersonSkill(_skill1, new Percent(1));
            _personSkill2 = new PersonSkill(_skill2, new Percent(1));
            _personSkill3 = new PersonSkill(_skill3, new Percent(1));
        }

        [Test]
        public void EmptyListOfShiftSwapDetailsReturnsTrue()
        {
            Assert.IsTrue(_target.IsSatisfiedBy(new List<IShiftTradeSwapDetail>()));
        }

        [Test]
        public void NoWorkflowControlSetReturnsFalse()
        {
            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(tradeDetails()));
            }
        }

        [Test]
        public void NoSkillsNeedsMatchReturnsTrue()
        {
            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill>())).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill>())).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(tradeDetails()));
            }
        }

        [Test]
        public void AllSkillsMatchReturnsTrue()
        {
            
            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill>{_skill1,_skill2})).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill>{_skill3})).Repeat.AtLeastOnce();

                Expect.Call(_personFrom.Period(new DateOnly())).IgnoreArguments().Return(_periodFrom);
                Expect.Call(_personTo.Period(new DateOnly())).IgnoreArguments().Return(_periodTo);

                Expect.Call(_periodFrom.PersonSkillCollection).Return(new List<IPersonSkill>
                                                                          {_personSkill1, _personSkill2, _personSkill3});
                Expect.Call(_periodTo.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1, _personSkill2, _personSkill3 });

            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(tradeDetails()));
            }

        }

        [Test]
        public void MissingPersonPeriodReturnsFalse()
        {

            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill1, _skill2 })).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill3 })).Repeat.AtLeastOnce();

                Expect.Call(_personFrom.Period(new DateOnly())).IgnoreArguments().Return(null);
                Expect.Call(_personTo.Period(new DateOnly())).IgnoreArguments().Return(_periodTo);

            }

            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(tradeDetails()));
            }

        }

        [Test]
        public void OneMissingSkillReturnsFalse()
        {

            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill1, _skill2 })).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill3 })).Repeat.AtLeastOnce();

                Expect.Call(_personFrom.Period(new DateOnly())).IgnoreArguments().Return(_periodFrom);
                Expect.Call(_personTo.Period(new DateOnly())).IgnoreArguments().Return(_periodTo);

                Expect.Call(_periodFrom.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 });
                Expect.Call(_periodTo.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1, _personSkill3 });
            }

            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(tradeDetails()));
            }

        }

        [Test]
        public void AnotherOneMissingSkillReturnsFalse()
        {
            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill1, _skill2 })).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill3 })).Repeat.AtLeastOnce();

                Expect.Call(_personFrom.Period(new DateOnly())).IgnoreArguments().Return(_periodFrom);
                Expect.Call(_personTo.Period(new DateOnly())).IgnoreArguments().Return(_periodTo);

                Expect.Call(_periodFrom.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1, _personSkill2, _personSkill3 });
                Expect.Call(_periodTo.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1, _personSkill2 });
            }

            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(tradeDetails()));
            }
        }

        [Test]
        public void AnotherOneHavingSameSkillsButNotInMatchingReturnsTrue()
        {
            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill3 })).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
                    new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill3 })).Repeat.AtLeastOnce();

                Expect.Call(_personFrom.Period(new DateOnly())).IgnoreArguments().Return(_periodFrom);
                Expect.Call(_personTo.Period(new DateOnly())).IgnoreArguments().Return(_periodTo);

                Expect.Call(_periodFrom.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill2 });
                Expect.Call(_periodTo.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1 });
            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(tradeDetails()));
            }
        }


        [Test]
        public void VerifyDenyReason()
        {
            Assert.AreEqual(_target.DenyReason,"ShiftTradeSkillDenyReason");
            Assert.IsNotNull(UserTexts.Resources.ShiftTradeSkillDenyReason);
        }

        private IList<IShiftTradeSwapDetail> tradeDetails()
        {
            IList<IShiftTradeSwapDetail> ret = new List<IShiftTradeSwapDetail>();
            IShiftTradeSwapDetail detail1 = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(), new DateOnly());
            
            ret.Add(detail1);
         

            return ret;
        }
    }

    
}
