using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class DateTimePeriodPerStartTimeSorterTest
	{
		private List<DateTimePeriod> periods;

		[SetUp]
		public void Setup()
		{
			periods=new List<DateTimePeriod>();
		}

		[Test]
		public void ShouldBeSortedOldestFirst()
		{
			periods.Add(new DateTimePeriod(2001,1,1,2010,2,1));
			periods.Add(new DateTimePeriod(2000,1,1,2000,1,2));
			periods.Add(new DateTimePeriod(2010,1,1,2012,1,2));

			periods.Sort(new DateTimePeriodPerStartTimeSorter());

			periods.Should().Have.SameSequenceAs(new[]
			                                     	{
			                                     		new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
			                                     		new DateTimePeriod(2001, 1, 1, 2010, 2, 1),
			                                     		new DateTimePeriod(2010, 1, 1, 2012, 1, 2)
			                                     	});
		}
	}
}