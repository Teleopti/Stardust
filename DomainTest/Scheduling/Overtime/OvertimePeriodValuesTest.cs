using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class OvertimePeriodValuesTest
	{
		private OvertimePeriodValues _target;
		private OvertimePeriodValue _overtimePeriodValue1;
		private OvertimePeriodValue _overtimePeriodValue2;
		private DateTimePeriod _dateTimePeriod1;
		private DateTimePeriod _dateTimePeriod2;
		private double _value1;
		private double _value2;

		[SetUp]
		public void SetUp()
		{
			_target = new OvertimePeriodValues();
			_dateTimePeriod1 = new DateTimePeriod(2014, 1, 1, 2014, 1, 2);
			_dateTimePeriod2 = new DateTimePeriod(2014, 1, 2, 2014, 1, 3);
			_value1 = 0.4;
			_value2 = 0.5;
			_overtimePeriodValue1 = new OvertimePeriodValue(_dateTimePeriod1, _value1);
			_overtimePeriodValue2 = new OvertimePeriodValue(_dateTimePeriod2, _value2);
			_target.Add(_overtimePeriodValue1);
			_target.Add(_overtimePeriodValue2);
		}

		[Test]
		public void ShouldGetTotalValue()
		{
			var result = _target.TotalValue();
			var expected = _value1 + _value2;
			Assert.AreEqual(expected, result);
		}
	}
}
