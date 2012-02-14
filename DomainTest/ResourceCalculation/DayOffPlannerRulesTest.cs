using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	
	[TestFixture]
	public class DayOffPlannerRulesTest
	{
		
		#region Variables
		
		// Variable to hold object to be tested for reuse by init functions
		private DayOffPlannerRules _target;
		
		#endregion
				
		#region SetUp and TearDown
		
		[SetUp]
		public void TestInit()
		{
			_target = new DayOffPlannerRules();
		    _target.Name = "sdsds";
		    _target.NumberOfDaysInPeriod = 14;
		}
		
		[TearDown]
		public void TestDispose()
		{
			_target = null;
		}
		
		#endregion
		
		#region Constructor Tests
		
		[Test]
		public void VerifyConstructor()
		{
			Assert.IsNotNull(_target);

		}
				
		#endregion
	
		#region Property Tests
		
        [Test]
		public void VerifyProperties()
		{
            Assert.AreEqual("sdsds", _target.Name);
            Assert.AreEqual(14,_target.NumberOfDaysInPeriod);

            _target.UseConsecutiveWorkdays = true;
            Assert.AreEqual(true, _target.UseConsecutiveWorkdays);
            _target.ConsecutiveDaysOff = new MinMax<int>(2,3);
            Assert.AreEqual(new MinMax<int>(2, 3), _target.ConsecutiveDaysOff);

            _target.UseConsecutiveDaysOff = true;
            Assert.AreEqual(true, _target.UseConsecutiveDaysOff);
            _target.ConsecutiveWorkdays = new MinMax<int>(2, 3);
            Assert.AreEqual(new MinMax<int>(2, 3), _target.ConsecutiveWorkdays);

            _target.UseDaysOffPerWeek = true;
            Assert.AreEqual(true, _target.UseDaysOffPerWeek);
            _target.DaysOffPerWeek = new MinMax<int>(2,5);
            Assert.AreEqual(new MinMax<int>(2, 5), _target.DaysOffPerWeek);

            _target.UseFreeWeekends = true;
            Assert.AreEqual(true, _target.UseFreeWeekends);
            _target.FreeWeekends = new MinMax<int>(1, 2);
            Assert.AreEqual(new MinMax<int>(1, 2), _target.FreeWeekends);

            _target.UseFreeWeekendDays = true;
            Assert.AreEqual(true, _target.UseFreeWeekendDays);
            _target.FreeWeekendDays = new MinMax<int>(1, 2);
            Assert.AreEqual(new MinMax<int>(1, 2), _target.FreeWeekendDays);

            _target.UsePostWeek = true;
            Assert.AreEqual(true,_target.UsePostWeek);

            _target.UsePreWeek = true;
            Assert.AreEqual(true, _target.UsePreWeek);

            _target.NumberOfDayOffsInPeriod = 6;
            Assert.AreEqual(6, _target.NumberOfDayOffsInPeriod);

            _target.KeepWeekendsTogether = true;
            Assert.AreEqual(true, _target.KeepWeekendsTogether);

            _target.MaximumMovableDayOffsPerPerson = 2;
            Assert.AreEqual(2, _target.MaximumMovableDayOffsPerPerson);
		}
		
		[Test]
        public void VerifyUseMoveMaxDays()
		{
            _target.MaximumMovableDayOffsPerPerson = 2;
            Assert.IsTrue(_target.UseMoveMaxDays);

            _target.MaximumMovableDayOffsPerPerson = 0;
            Assert.IsFalse(_target.UseMoveMaxDays);

		}


	    #endregion
			
	}

}


