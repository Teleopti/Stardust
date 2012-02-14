using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
	[TestFixture]
	public class PositiveTimeSpanTest
	{
		private PositiveTimeSpan _target;

		public void Setup()
		{
			_target = new PositiveTimeSpan();
		}

		[Test]
		public void ShouldConstructCorrectly()
		{
			TimeSpan timeSpan = new TimeSpan(2,3,5);
			_target = new PositiveTimeSpan(timeSpan);

			Assert.AreEqual(timeSpan, _target.TimeSpan);

			_target = new PositiveTimeSpan(5,8,13);

			Assert.AreEqual(5, _target.TimeSpan.Hours);
			Assert.AreEqual(8, _target.TimeSpan.Minutes);
			Assert.AreEqual(13, _target.TimeSpan.Seconds);
		}

		[Test]
		public void ShouldThrowExceptionForNegativeTimeSpan()
		{
			TimeSpan unvalid = new TimeSpan(-3, 0, 0);
			TimeSpan valid = new TimeSpan(5, 6, 49);

			Assert.Throws<ArgumentOutOfRangeException>(delegate { _target.TimeSpan = unvalid; });
			Assert.Throws<ArgumentOutOfRangeException>(delegate { _target = (PositiveTimeSpan) unvalid; });
			Assert.DoesNotThrow(delegate { _target.TimeSpan = valid; });
			Assert.DoesNotThrow(delegate { _target = (PositiveTimeSpan)valid; });
		}

		[Test]
		public void ShouldCompareCorrectly()
		{
			_target = new PositiveTimeSpan(12, 0, 0);

			var equals = new PositiveTimeSpan(12, 0, 0);
			var less = new PositiveTimeSpan(8, 23, 45);
			var greater = new PositiveTimeSpan(12, 0, 2);

			Assert.True(equals == _target);
			Assert.True(less < _target);
			Assert.True(greater > _target);
			Assert.True(equals <= _target);
			Assert.True(less <= _target);
			Assert.False(greater <= _target);
			Assert.True(equals >= _target);
			Assert.False(less >= _target);
			Assert.True(greater >= _target);
			Assert.True(greater != _target);
			Assert.False(equals != _target);
			Assert.True(_target.Equals(equals));
			Assert.False(_target.Equals(less));
			Assert.AreEqual(0, PositiveTimeSpan.Compare(_target, equals));
			Assert.AreEqual(1, PositiveTimeSpan.Compare(_target, less));
			Assert.AreEqual(-1, PositiveTimeSpan.Compare(_target, greater));
			Assert.True(_target.Equals((Object) equals));
			Assert.False(_target.Equals((Object) less));
			Assert.False(_target.Equals((Object) "test"));
			Assert.False(_target.Equals(null));
		}

		[Test]
		public void ShouldReturnCorrectHashCode()
		{
			TimeSpan timeSpan = new TimeSpan(2, 3, 5);
			_target = new PositiveTimeSpan(timeSpan);

			Assert.AreEqual(timeSpan.GetHashCode(), _target.GetHashCode());
		}

		[Test]
		public void ShouldConvertToTimeSpan()
		{
			TimeSpan timeSpan = new TimeSpan(5, 8, 13);
			_target = new PositiveTimeSpan(timeSpan);

			Assert.AreEqual(timeSpan, PositiveTimeSpan.ToTimeSpan(_target));
		}
	}
}
