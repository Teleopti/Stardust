using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class RuleSetDeletedShiftCategoryCheckerTest
	{
		private MockRepository _mocks;
		private IShiftCategory _shiftCategory;
		private IWorkShiftRuleSet _ruleSet;
		private IWorkShiftTemplateGenerator _templateGenerator;
		private RuleSetDeletedShiftCategoryChecker _ruleSetDeletedShiftCategoryChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_shiftCategory = new ShiftCategory("shiftCategory");
			_templateGenerator = _mocks.StrictMock<IWorkShiftTemplateGenerator>();
			_ruleSetDeletedShiftCategoryChecker = new RuleSetDeletedShiftCategoryChecker();
			_ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionIfRuleSetIsNull()
		{
			_ruleSetDeletedShiftCategoryChecker.ContainsDeletedShiftCategory(null);
		}

		[Test]
		public void ShouldReturnFalseIfShiftCategoryIsNotDeleted()
		{
			Expect.Call(_ruleSet.TemplateGenerator).Return(_templateGenerator);
			Expect.Call(_templateGenerator.Category).Return(_shiftCategory);
			
			_mocks.ReplayAll();

			var result = _ruleSetDeletedShiftCategoryChecker.ContainsDeletedShiftCategory(_ruleSet);
			Assert.That(result, Is.False);

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnTrueIfShiftCategoryIsDeleted()
		{
			Expect.Call(_ruleSet.TemplateGenerator).Return(_templateGenerator);
			Expect.Call(_templateGenerator.Category).Return(_shiftCategory);

			_mocks.ReplayAll();

			((IDeleteTag)_shiftCategory).SetDeleted();
			var result = _ruleSetDeletedShiftCategoryChecker.ContainsDeletedShiftCategory(_ruleSet);
			Assert.That(result, Is.True);

			_mocks.VerifyAll();		
		}
	}
}
