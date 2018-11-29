using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class BusinessRuleResponseContainsDateSpecificationTest
	{
		private BusinessRuleResponseContainsDateSpecification _target;
		private IEnumerable<IShiftTradeSwapDetail> _shiftTradeSwapDetails;
		private IShiftTradeSwapDetail _shiftTradeSwapDetail1;
		private IShiftTradeSwapDetail _shiftTradeSwapDetail2;
		private IPerson _person;
		private DateOnly _dateFrom1;
		private DateOnly _dateTo1;
		private DateOnly _dateFrom2;
		private DateOnly _dateTo2;
		private DateOnly _dateNotContained;
		private IBusinessRuleResponse _businessRuleResponse;


		[SetUp]
		public void Setup()
		{
			_dateFrom1 = new DateOnly(2011, 02, 01);
			_dateTo1 = new DateOnly(2011, 02, 02);

			_dateNotContained = new DateOnly(2011, 02, 03);

			_dateFrom2 = new DateOnly(2011, 02, 04);
			_dateTo2 = new DateOnly(2011, 02, 05);

			_person = PersonFactory.CreatePerson();
			_shiftTradeSwapDetail1 = new ShiftTradeSwapDetail(_person, _person, _dateFrom1, _dateTo1);
			_shiftTradeSwapDetail2 = new ShiftTradeSwapDetail(_person, _person, _dateFrom2, _dateTo2);

			_shiftTradeSwapDetails = new List<IShiftTradeSwapDetail> { _shiftTradeSwapDetail1, _shiftTradeSwapDetail2 };
			_target = new BusinessRuleResponseContainsDateSpecification(new List<IShiftTradeSwapDetail>(_shiftTradeSwapDetails));

		}

		[Test]
		public void BusinessRuleResponseContainsTheFirstOnly()
		{
			_businessRuleResponse =
				new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true, new DateTimePeriod(), _person, new DateOnlyPeriod(_dateFrom1, _dateNotContained), "tjillevippen");
			Assert.IsTrue(_target.IsSatisfiedBy(_businessRuleResponse));
		}

		[Test]
		public void BusinessRuleResponseContainsTheSecondOnly()
		{
			_businessRuleResponse =
				new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true, new DateTimePeriod(), _person, new DateOnlyPeriod(_dateNotContained, _dateTo2), "tjillevippen");
			Assert.IsTrue(_target.IsSatisfiedBy(_businessRuleResponse));
		}

		[Test]
		public void BusinessRuleResponseNotContains()
		{
			_businessRuleResponse =
				new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true, new DateTimePeriod(), _person, new DateOnlyPeriod(_dateNotContained, _dateNotContained), "tjillevippen");
			Assert.IsFalse(_target.IsSatisfiedBy(_businessRuleResponse));
		}

		[Test]
		public void BusinessRuleResponseContainsBoth()
		{
			_businessRuleResponse =
				new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true, new DateTimePeriod(), _person, new DateOnlyPeriod(_dateFrom1, _dateTo2), "tjillevippen");
			Assert.IsTrue(_target.IsSatisfiedBy(_businessRuleResponse));
		}
	}
}
