using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;


namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class DayRangeOptimizerTest
	{
		[Test]
		public void ShoudlReduceRanges()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var personId4 = Guid.NewGuid();
			var personId5 = Guid.NewGuid();
			var personId6 = Guid.NewGuid();
			var personId7 = Guid.NewGuid();
			var input = new List<PersonDayProjectionChanged>
			{
				/* |-----|
				 * |_____|   <- Should become */
				new PersonDayProjectionChanged(personId1, new DateOnly(2019,02,01), new DateOnly(2019,02,10)),

				/* |-| |--| |---|
				 * |_| |__| |___|   <- Should become */
				new PersonDayProjectionChanged(personId2, new DateOnly(2019,02,01), new DateOnly(2019,02,01)),
				new PersonDayProjectionChanged(personId2, new DateOnly(2019,02,03), new DateOnly(2019,02,04)),
				new PersonDayProjectionChanged(personId2, new DateOnly(2019,02,06), new DateOnly(2019,02,09)),

				/* |-|    |---|
				 *    |-|      |-|
				 * |_____________|   <- Should become */
				new PersonDayProjectionChanged(personId3, new DateOnly(2019,02,01), new DateOnly(2019,02,01)),
				new PersonDayProjectionChanged(personId3, new DateOnly(2019,02,02), new DateOnly(2019,02,02)),
				new PersonDayProjectionChanged(personId3, new DateOnly(2019,02,03), new DateOnly(2019,02,06)),
				new PersonDayProjectionChanged(personId3, new DateOnly(2019,02,07), new DateOnly(2019,02,07)),

				/* |---|      |---| 
				 *    |---| |---|    
				 * |______| |_____|   <- Should become */
				new PersonDayProjectionChanged(personId4, new DateOnly(2019,02,01), new DateOnly(2019,02,05)),
				new PersonDayProjectionChanged(personId4, new DateOnly(2019,02,03), new DateOnly(2019,02,07)),
				new PersonDayProjectionChanged(personId4, new DateOnly(2019,02,10), new DateOnly(2019,02,13)),
				new PersonDayProjectionChanged(personId4, new DateOnly(2019,02,12), new DateOnly(2019,02,16)),
				
				/*   |---|   |-------|
				 * |-------|   |---|
				 * |_______| |_______|   <- Should become */
				new PersonDayProjectionChanged(personId5, new DateOnly(2019,03,01), new DateOnly(2019,03,31)),
				new PersonDayProjectionChanged(personId5, new DateOnly(2019,03,04), new DateOnly(2019,03,27)),
				new PersonDayProjectionChanged(personId5, new DateOnly(2019,04,14), new DateOnly(2019,05,14)),
				new PersonDayProjectionChanged(personId5, new DateOnly(2019,04,24), new DateOnly(2019,05,07)),

				/* |-------|   |-------|  |---------|
				 *         |------|        |------|
				 *            |------|              |----|
				 * |___________________|  |______________|   <- Should be become  */
				new PersonDayProjectionChanged(personId6, new DateOnly(2019,02,01), new DateOnly(2019,02,05)),
				new PersonDayProjectionChanged(personId6, new DateOnly(2019,02,05), new DateOnly(2019,02,15)),
				new PersonDayProjectionChanged(personId6, new DateOnly(2019,02,07), new DateOnly(2019,02,20)),
				new PersonDayProjectionChanged(personId6, new DateOnly(2019,02,08), new DateOnly(2019,02,25)),
				new PersonDayProjectionChanged(personId6, new DateOnly(2019,02,28), new DateOnly(2019,03,10)),
				new PersonDayProjectionChanged(personId6, new DateOnly(2019,03,02), new DateOnly(2019,03,08)),
				new PersonDayProjectionChanged(personId6, new DateOnly(2019,03,08), new DateOnly(2019,03,15)),

				/*   |-|   |-|
				 *      |-|    |--|
				 * |------------------|
				 * |__________________|   <- Should be become  */
				new PersonDayProjectionChanged(personId7, new DateOnly(2019,02,04), new DateOnly(2019,02,04)),
				new PersonDayProjectionChanged(personId7, new DateOnly(2019,02,05), new DateOnly(2019,02,05)),
				new PersonDayProjectionChanged(personId7, new DateOnly(2019,02,06), new DateOnly(2019,02,06)),
				new PersonDayProjectionChanged(personId7, new DateOnly(2019,02,08), new DateOnly(2019,02,10)),
				new PersonDayProjectionChanged(personId7, new DateOnly(2019,02,01), new DateOnly(2019,02,15))
			};

			var reduced = DayRangeOptimizer.Reduce(input);

			reduced.Where(p => p.PersonId == personId1).Count().Should().Be.EqualTo(1);
			reduced.Where(p => p.PersonId == personId2).Count().Should().Be.EqualTo(3);
			reduced.Where(p => p.PersonId == personId3).Count().Should().Be.EqualTo(1); 
			reduced.Where(p => p.PersonId == personId4).Count().Should().Be.EqualTo(2);
			reduced.Where(p => p.PersonId == personId5).Count().Should().Be.EqualTo(2);
			reduced.Where(p => p.PersonId == personId6).Count().Should().Be.EqualTo(2);
			reduced.Where(p => p.PersonId == personId7).Count().Should().Be.EqualTo(1);
		}


		[Test]
		public void ShoudPageProjectionDays()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var input = new List<PersonDayProjectionChanged>
			{
				new PersonDayProjectionChanged(personId2, new DateOnly(2019,03,06), new DateOnly(2019,03,10)), // 7. 5
				new PersonDayProjectionChanged(personId1, new DateOnly(2019,02,01), new DateOnly(2019,02,03)), // 1. 3
				new PersonDayProjectionChanged(personId2, new DateOnly(2019,02,05), new DateOnly(2019,02,07)), // 2. 3
				new PersonDayProjectionChanged(personId1, new DateOnly(2019,02,09), new DateOnly(2019,02,14)), // 3. 6
				new PersonDayProjectionChanged(personId2, new DateOnly(2019,02,16), new DateOnly(2019,02,20)), // 4. 5
				new PersonDayProjectionChanged(personId2, new DateOnly(2019,02,22), new DateOnly(2019,02,25)), // 5. 4
				new PersonDayProjectionChanged(personId1, new DateOnly(2019,02,27), new DateOnly(2019,03,04)), // 6. 6
			};

			Assert.Throws<FaultException>(() => DayRangeOptimizer.PageByProjectionDays(input, -1, 10));
			Assert.Throws<FaultException>(() => DayRangeOptimizer.PageByProjectionDays(input, 1, 0));

			var r1 = DayRangeOptimizer.PageByProjectionDays(input, 1, 6);
			r1.TotalSchedules.Should().Be(32);
			r1.DaysInPage.Should().Be(6);
			r1.TotalPages.Should().Be(5);
			r1.Projections.Count.Should().Be(2);

			r1 = DayRangeOptimizer.PageByProjectionDays(input, 1, 1);
			r1.TotalSchedules.Should().Be(32);
			r1.DaysInPage.Should().Be(3);
			r1.TotalPages.Should().Be(7);
			r1.Projections.Count.Should().Be(1);

			r1 = DayRangeOptimizer.PageByProjectionDays(input, 1, 11);
			r1.TotalSchedules.Should().Be(32);
			r1.DaysInPage.Should().Be(12);
			r1.TotalPages.Should().Be(3);
			r1.Projections.Count.Should().Be(3);

			r1 = DayRangeOptimizer.PageByProjectionDays(input, 2, 11);
			r1.TotalSchedules.Should().Be(32);
			r1.DaysInPage.Should().Be(15);
			r1.TotalPages.Should().Be(3);
			r1.Projections.Count.Should().Be(3);

			r1 = DayRangeOptimizer.PageByProjectionDays(input, 3, 11);
			r1.TotalSchedules.Should().Be(32);
			r1.DaysInPage.Should().Be(5);
			r1.TotalPages.Should().Be(3);
			r1.Projections.Count.Should().Be(1);
			
			r1 = DayRangeOptimizer.PageByProjectionDays(input, 4, 11);
			r1.TotalSchedules.Should().Be(32);
			r1.DaysInPage.Should().Be(0);
			r1.TotalPages.Should().Be(3);
			r1.Projections.Count.Should().Be(0);
		}
	}
}