using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
	[TestFixture]
	public class WorkShiftAddCallbackTest
	{
		private MockRepository _mocks;
		private WorkShiftAddCallback _target;
		private IWorkShiftRuleSet _ruleSet;
		private IWorkShift _workShift;
		private int _cnt;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
			_workShift = _mocks.DynamicMock<IWorkShift>();
			_target = new WorkShiftAddCallback();
			_target.CountChanged += targetCountChanged;
			_target.RuleSetReady += targetOnRuleSetReady;
			_target.RuleSetToComplex += _target_RuleSetToComplex;
			_target.RuleSetWarning += _target_RuleSetWarning;
		}

		void _target_RuleSetWarning(object sender, EventArgs e)
		{}

		void _target_RuleSetToComplex(object sender, ComplexRuleSetEventArgs e)
		{}

		private void targetOnRuleSetReady(object sender, EventArgs eventArgs)
		{
		}

		void targetCountChanged(object sender, EventArgs e)
		{
			_cnt = _target.CurrentCount;
		}

		[Test]
		public void ShouldResetOnNewRuleSet()
		{
			Expect.Call(_ruleSet.Description).Return(new Description("itt"));
			Expect.Call(_ruleSet.Description).Return(new Description("två"));
			_mocks.ReplayAll();
			_target.StartNewRuleSet(_ruleSet);
			
			_target.BeforeAdd(_workShift);
			_target.BeforeAdd(_workShift);
			_target.BeforeAdd(_workShift);
			Assert.That(_target.CurrentRuleSetName, Is.EqualTo("itt"));
			Assert.That(_target.CurrentCount,Is.EqualTo(3));
			_target.StartNewRuleSet(_ruleSet);
			_target.BeforeAdd(_workShift);
			Assert.That(_target.CurrentCount, Is.EqualTo(1));
			_target.BeforeRemove();
			Assert.That(_target.CurrentCount, Is.EqualTo(0));
			Assert.That(_target.CurrentRuleSetName,Is.EqualTo("två"));
		}

		[Test]
		public void ShouldResetCancelOnEndRuleSet()
		{
			Expect.Call(_ruleSet.Description).Return(new Description("itt"));
			_mocks.ReplayAll();
			_target.StartNewRuleSet(_ruleSet);
			_target.Cancel();
			Assert.That(_target.IsCanceled, Is.True);
			_target.EndRuleSet();
			Assert.That(_target.IsCanceled, Is.False);
			
		}

		[Test]
		public void ShouldGetCountEventAfterFifty()
		{
			_cnt = 0;
			for (var i = 0; i < 50; i++)
			{
				_target.BeforeAdd(_workShift);
			}
			Assert.That(_cnt,Is.GreaterThan(0));
		}
	}

	
}