using System;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
	[TestFixture]
	public class ActualAgentStateTest
	{
		private ActualAgentState _target;

		[SetUp]
		public void Setup()
		{
			_target = new ActualAgentState();
		}

		[Test]
		public void VerifyProperties()
		{
			var guid = Guid.NewGuid();
			var dateTime = DateTime.UtcNow;
			var timeSpan = TimeSpan.FromMinutes(45);

			_target.PersonId = guid;
			_target.State = "State";
			_target.StateId = guid;
			_target.Scheduled = "Scheduled";
			_target.StateStart = dateTime;
			_target.ScheduledNext = "ScheduledNext";
			_target.ScheduledNextId = guid;
			_target.NextStart = dateTime;
			_target.AlarmName = "AlarmName";
			_target.Color = 123456789;
			_target.AlarmStart = dateTime;
			_target.StaffingEffect = 10D;
			_target.StateCode = "StateCode";
			_target.ScheduledId = guid;
			_target.PlatformTypeId = guid;
			_target.Timestamp = dateTime;
			_target.TimeInState = timeSpan;

			Assert.That(_target.State, Is.EqualTo("State"));
			Assert.That(_target.PersonId, Is.EqualTo(guid));
			Assert.That(_target.StateId, Is.EqualTo(guid));
			Assert.That(_target.Scheduled, Is.EqualTo("Scheduled"));
			Assert.That(_target.StateStart, Is.EqualTo(dateTime));
			Assert.That(_target.ScheduledNext, Is.EqualTo("ScheduledNext"));
			Assert.That(_target.ScheduledNextId, Is.EqualTo(guid));
			Assert.That(_target.NextStart, Is.EqualTo(dateTime));
			Assert.That(_target.AlarmName, Is.EqualTo("AlarmName"));
			Assert.That(_target.Color, Is.EqualTo(123456789));
			Assert.That(_target.AlarmStart, Is.EqualTo(dateTime));
			Assert.That(_target.StaffingEffect, Is.EqualTo(10D));
			Assert.That(_target.StateCode, Is.EqualTo("StateCode"));
			Assert.That(_target.ScheduledId, Is.EqualTo(guid));
			Assert.That(_target.PlatformTypeId, Is.EqualTo(guid));
			Assert.That(_target.Timestamp, Is.EqualTo(dateTime));
			Assert.That(_target.TimeInState, Is.EqualTo(timeSpan));
		}

		[Test]
		public void ShouldReturnEquals()
		{
			// ReSharper disable ExpressionIsAlwaysNull
			IActualAgentState other = null;
			Assert.IsFalse(_target.Equals(other));
			// ReSharper restore ExpressionIsAlwaysNull
			VerifyProperties();
			other = new ActualAgentState();
			Assert.IsFalse(_target.Equals(other));

			other = _target;
			Assert.IsTrue(_target.Equals(other));
		}
	}
}
