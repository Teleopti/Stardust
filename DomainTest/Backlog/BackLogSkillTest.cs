using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Backlog
{
	[TestFixture]
	public class BacklogSkillTest
	{
		private BacklogSkill _target;
		private IServiceLevel _serviceLevel;

		[SetUp]
		public void Setup()
		{
			_target = new BacklogSkill();
			_serviceLevel = new ServiceLevel(new Percent(1), TimeSpan.FromDays(2).TotalSeconds);
		}

		[Test]
		public void ShouldReturnOrderedListOfAffectedTasksForSpecificDate()
		{
			_target.AddIncomingTask(new DateOnly(2015, 3, 21), _serviceLevel, 1, TimeSpan.FromHours(1));
			_target.AddIncomingTask(new DateOnly(2015, 3, 20), _serviceLevel, 1, TimeSpan.FromHours(1));
			_target.AddIncomingTask(new DateOnly(2015, 3, 19), _serviceLevel, 1, TimeSpan.FromHours(1));
			var ret = _target.TasksAffected(new DateOnly(2015, 3, 21));
			Assert.AreEqual(2, ret.Count());
			Assert.AreEqual(new DateOnly(2015, 3, 20), ret.First().SpanningPeriod.StartDate);
		}
	}
}