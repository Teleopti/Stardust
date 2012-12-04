﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class OpenShiftTradePeriodSpecificationTest
	{
		private OpenShiftTradePeriodSpecification _target;
		private IPerson _personFrom;
		private IPerson _personTo;

		[SetUp]
		public void Setup()
		{
			_target = new OpenShiftTradePeriodSpecification();
			var wcs = new WorkflowControlSet("wcs") { ShiftTradeOpenPeriodDaysForward = new MinMax<int>(1, 99) };
			_personFrom = PersonFactory.CreatePerson("test person from");
			_personFrom.WorkflowControlSet = wcs;
			_personTo = PersonFactory.CreatePerson("test person to");
			_personTo.WorkflowControlSet = wcs;
		}

		[Test]
		public void ShouldBeWrongIfOutsideOfOpenPeriod()
		{
			var checkItem = new ShiftTradeAvailableCheckItem { DateOnly = DateOnly.Today, PersonFrom = _personFrom, PersonTo = _personTo };
			Assert.That(_target.IsSatisfiedBy(checkItem), Is.False);
		}

		[Test]
		public void ShouldBeRightIfInsideOfOpenPeriod()
		{
			var checkItem = new ShiftTradeAvailableCheckItem { DateOnly = DateOnly.Today.AddDays(1), PersonFrom = _personFrom, PersonTo = _personTo };
			Assert.That(_target.IsSatisfiedBy(checkItem), Is.True);
		}

		[Test]
		public void ShouldFailIfOneHasNoWorkflowControlSet()
		{
			_personFrom.WorkflowControlSet = null;
			_personTo.WorkflowControlSet = new WorkflowControlSet();
			var checkItem = new ShiftTradeAvailableCheckItem { DateOnly = DateOnly.Today, PersonFrom = _personFrom, PersonTo = _personTo };
			Assert.That(_target.IsSatisfiedBy(checkItem), Is.False);
		}
	}
}
