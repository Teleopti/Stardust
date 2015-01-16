using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	public class MinWeekWorkTimeRuleToggle31992OffTest
	{
		private MinWeekWorkTimeRuleToggle31992Off _target;
		private IDictionary<IPerson, IScheduleRange> _dictionary;
		private IList<IScheduleDay> _scheduleDays;
		
		[SetUp]
		public void Setup()
		{
			_target = new MinWeekWorkTimeRuleToggle31992Off();
			_dictionary = new Dictionary<IPerson, IScheduleRange>();
			_scheduleDays = new List<IScheduleDay>();
		}

		[Test]
		public void ShouldReturnEmptyResponseOnValidate()
		{
			var response = _target.Validate(_dictionary, _scheduleDays);
			Assert.IsEmpty(response);
		}

		[Test]
		public void ShouldReturnEmptyErrorMessage()
		{
			var errorMessage = _target.ErrorMessage;
			Assert.AreEqual(string.Empty, errorMessage);
		}

		[Test]
		public void ShouldReturnNotManadatory()
		{
			var mandatory = _target.IsMandatory;
			Assert.IsFalse(mandatory);
		}
	}
}
