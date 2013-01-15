using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ShiftCategoryFairnessValueHelperTest
	{
		private ShiftCategoryFairnessValueHelper _target;
		private ShiftCategoryFairnessCompareValue _agentValue;
		private ShiftCategoryFairnessCompareValue _teamValue;
		private IList<IShiftCategoryFairnessCompareValue> _agentValues;
		private IList<IShiftCategoryFairnessCompareValue> _teamValues;
		private IShiftCategory _shiftCategory1;
		private IShiftCategory _shiftCategory2;
		private IShiftCategory _shiftCategory3;

		[SetUp]
		public void Setup()
		{
			_shiftCategory1 = new ShiftCategory("shiftCategory1");
			_shiftCategory2 = new ShiftCategory("shiftCategory2");
			_shiftCategory3 = new ShiftCategory("shiftCategory2");

			_shiftCategory1.SetId(Guid.NewGuid());
			_shiftCategory2.SetId(Guid.NewGuid());
			_shiftCategory3.SetId(Guid.NewGuid());

			_agentValue = new ShiftCategoryFairnessCompareValue();
			_teamValue = new ShiftCategoryFairnessCompareValue();

			_agentValue.ShiftCategory = _shiftCategory1;
			_teamValue.ShiftCategory = _shiftCategory2;

			_agentValues = new List<IShiftCategoryFairnessCompareValue>{_agentValue};
			_teamValues = new List<IShiftCategoryFairnessCompareValue>{_teamValue};

			_target = new ShiftCategoryFairnessValueHelper(_agentValues, _teamValues);	
		}

		[Test]
		public void ShouldGetShiftCategories()
		{
			var shiftCategories = _target.ShiftCategories();
			Assert.AreEqual(2, shiftCategories.Count);
			Assert.IsTrue(shiftCategories.Contains(_shiftCategory1));
			Assert.IsTrue(shiftCategories.Contains(_shiftCategory2));
		}

		[Test]
		public void ShouldGetValuesOnShiftCategory()
		{
			var valueAgent = _target.AgentValue(_shiftCategory1);
			var valueTeam = _target.TeamValue(_shiftCategory2);

			Assert.AreEqual(valueAgent, _agentValue);
			Assert.AreEqual(valueTeam, _teamValue);
		}

		[Test]
		public void ShouldGetEmptyValuesOnNoHit()
		{
			var valueAgent = _target.AgentValue(_shiftCategory3);
			var valueTeam = _target.TeamValue(_shiftCategory3);

			Assert.IsNull(valueAgent.ShiftCategory);
			Assert.IsNull(valueTeam.ShiftCategory);

			Assert.AreEqual(0d, valueAgent.Original);
			Assert.AreEqual(0d, valueAgent.ComparedTo);

			Assert.AreEqual(0d, valueTeam.Original);
			Assert.AreEqual(0d, valueTeam.ComparedTo);
		}
	}
}
