using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;


namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradeSkillSpecificationTest
	{
		private ShiftTradeSkillSpecification _target;
		private MockRepository _mocks;
		private IPerson _personFrom;
		private IPerson _personTo;
		private IWorkflowControlSet _workflowControlSetFrom;
		private IWorkflowControlSet _workflowControlSetTo;
		private IPersonPeriod _periodFrom;
		private IPersonPeriod _periodTo;
		private ISkill _skill1;
		private ISkill _skill2;
		private ISkill _skill3;
		private PersonSkill _personSkill1;
		private PersonSkill _personSkill2;
		private PersonSkill _personSkill3;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new ShiftTradeSkillSpecification();
			_personFrom = _mocks.StrictMock<IPerson>();
			_personTo = _mocks.StrictMock<IPerson>();
			_workflowControlSetFrom = _mocks.StrictMock<IWorkflowControlSet>();
			_workflowControlSetTo = _mocks.StrictMock<IWorkflowControlSet>();
			_periodFrom = _mocks.StrictMock<IPersonPeriod>();
			_periodTo = _mocks.StrictMock<IPersonPeriod>();

			_skill1 = _mocks.StrictMock<ISkill>();
			_skill2 = _mocks.StrictMock<ISkill>();
			_skill3 = _mocks.StrictMock<ISkill>();

			_personSkill1 = new PersonSkill(_skill1, new Percent(1));
			_personSkill2 = new PersonSkill(_skill2, new Percent(1));
			_personSkill3 = new PersonSkill(_skill3, new Percent(1));
		}

		[Test]
		public void ShouldHaveCorrectDenyReasonSet()
		{
			_target.DenyReason.Should().Be.EqualTo("ShiftTradeSkillDenyReason");
		}

		[Test]
		public void NoWorkflowControlSetReturnsFalse()
		{
			var checkItem = new ShiftTradeAvailableCheckItem(new DateOnly(), _personFrom, _personTo);

			using (_mocks.Record())
			{
				Expect.Call(_personFrom.WorkflowControlSet).Return(null).Repeat.AtLeastOnce();
				Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.IsSatisfiedBy(checkItem));
			}
		}

		[Test]
		public void ShouldPassIfNoMatchingSkillSetup()
		{
			var checkItem = new ShiftTradeAvailableCheckItem(new DateOnly(), _personFrom, _personTo);

			using (_mocks.Record())
			{
				Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
					 new ReadOnlyCollection<ISkill>(new List<ISkill>())).Repeat.AtLeastOnce();
				Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
					 new ReadOnlyCollection<ISkill>(new List<ISkill>())).Repeat.AtLeastOnce();
				Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
				Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				Assert.That(_target.IsSatisfiedBy(checkItem), Is.True);
			}
		}

		[Test]
		public void ShouldHaveAllMatchingSkills()
		{
			var dateOnly = DateOnly.Today.AddDays(1);
			var checkItem = new ShiftTradeAvailableCheckItem(dateOnly, _personFrom, _personTo);

			using (_mocks.Record())
			{
				Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
					 new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill1, _skill2 })).Repeat.AtLeastOnce();
				Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
					 new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill3 })).Repeat.AtLeastOnce();
				Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
				Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
				Expect.Call(_personFrom.Period(dateOnly)).Return(_periodFrom);
				Expect.Call(_personTo.Period(dateOnly)).Return(_periodTo);
				Expect.Call(_periodFrom.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1, _personSkill2, _personSkill3 });
				Expect.Call(_periodTo.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1, _personSkill2, _personSkill3 });
			}
			using (_mocks.Playback())
			{
				Assert.That(_target.IsSatisfiedBy(checkItem), Is.True);
			}
		}

		[Test]
		public void ShouldNoticeMissingSkill()
		{
			var dateOnly = DateOnly.Today.AddDays(1);
			var checkItem = new ShiftTradeAvailableCheckItem(dateOnly, _personFrom, _personTo);
			using (_mocks.Record())
			{
				Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
					 new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill1, _skill2 })).Repeat.AtLeastOnce();
				Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
					 new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill3 })).Repeat.AtLeastOnce();
				Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
				Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
				Expect.Call(_personFrom.Period(dateOnly)).Return(_periodFrom);
				Expect.Call(_personTo.Period(dateOnly)).Return(_periodTo);
				Expect.Call(_periodFrom.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1, _personSkill3 });
				Expect.Call(_periodTo.PersonSkillCollection).Return(new List<IPersonSkill> { _personSkill1, _personSkill2, _personSkill3 });
			}
			using (_mocks.Playback())
			{
				Assert.That(_target.IsSatisfiedBy(checkItem), Is.False);
			}
		}

		[Test]
		public void ShouldFailIfNoPersonPeriodAvailable()
		{
			var dateOnly = DateOnly.Today.AddDays(1);
			var checkItem = new ShiftTradeAvailableCheckItem(dateOnly, _personFrom, _personTo);
			using (_mocks.Record())
			{
				Expect.Call(_workflowControlSetFrom.MustMatchSkills).Return(
					 new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill1, _skill2 })).Repeat.AtLeastOnce();
				Expect.Call(_workflowControlSetTo.MustMatchSkills).Return(
					 new ReadOnlyCollection<ISkill>(new List<ISkill> { _skill3 })).Repeat.AtLeastOnce();
				Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
				Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
				Expect.Call(_personFrom.Period(dateOnly)).Return(null);
				Expect.Call(_personTo.Period(dateOnly)).Return(_periodTo);
			}
			using (_mocks.Playback())
			{
				Assert.That(_target.IsSatisfiedBy(checkItem), Is.False);
			}
		}
	}
}
