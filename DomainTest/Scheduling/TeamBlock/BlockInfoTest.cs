using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class BlockInfoTest
	{
		//private MockRepository _mocks;
		private IBlockInfo _target;

		[SetUp]
		public void Setup()
		{
			//_mocks = new MockRepository();
			_target = new BlockInfo(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 27)));
		}

		[Test]
		public void ShouldReturnBlockPeriod()
		{
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 27)), _target.BlockPeriod); 
		}

		[Test]
		public void ShouldHoldStandardDeviationValues()
		{
			var values = new List<double?> {0.1, 0.2, null, 0.3};
			_target.StandardDeviations = values;

			Assert.That(_target.StandardDeviations, Is.EqualTo(values));
		}
		
		[Test]
		public void ShouldGetSumOfStandardDeviations()
		{
			var values = new List<double?> {0.1, 0.2, null, 0.3};
			_target.StandardDeviations = values;

			Assert.That(Math.Round(_target.Sum, 1), Is.EqualTo(0.6));
		}
		
		[Test]
		public void ShouldGetAverageOfStandardDeviations()
		{
			var values = new List<double?> {0.1, 0.2, null, 0.3};
			_target.StandardDeviations = values;

			Assert.That(Math.Round(_target.Average, 1), Is.EqualTo(0.2));
		}
	}
}