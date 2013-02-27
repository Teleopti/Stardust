using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class IntradayBlockTest
	{
		private IIntradayBlock _target;

		[SetUp]
		public void Setup()
		{
			_target = new IntradayBlock();
		}

		[Test]
		public void ShouldHaveCoveringPeriod()
		{
			var date = new DateOnly();
			_target.BlockDays = new List<DateOnly>
				{
					date,
					date.AddDays(1),
					date.AddDays(3)
				};

			var period = new DateOnlyPeriod(date, date.AddDays(3));
			
			Assert.That(_target.CoveringPeriod, Is.EqualTo(period));
		}


	}
}
