using System;
using NUnit.Framework;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class PersonStateHolderTest
	{
		private PersonStateHolder _target, _other;
		private DateTime _dateTime;

		[SetUp]
		public void Setup()
		{
			_dateTime = DateTime.Now;
		}

		[Test]
		public void ShouldReturnTrueForSameVariables()
		{
			_target = new PersonStateHolder("state", _dateTime);
			_other = new PersonStateHolder("state", _dateTime);

			Assert.That(_target.Equals(_other), Is.True);
		}

		[Test]
		public void ShouldReturnFalseForDifferentVariables()
		{
			_target = new PersonStateHolder("OneState", _dateTime.AddMinutes(-5));
			_target = new PersonStateHolder("OtherState", _dateTime.AddMinutes(5));
			Assert.That(_target.Equals(_other), Is.False);
		}
	}
}
