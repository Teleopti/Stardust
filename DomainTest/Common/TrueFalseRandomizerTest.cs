using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class TrueFalseRandomizerTest
	{
		private ITrueFalseRandomizer _target;

		[SetUp]
		public void Setup()
		{
			_target = new TrueFalseRandomizer();
		}

		[Test]
		public void ShouldRandomize()
		{
			IList<bool> result = new List<bool>();
			for (int i = 0; i < 100; i++)
			{
				if(_target.Randomize(i))
					result.Add(true);
			}

			Assert.IsTrue(result.Count > 25);
		}
	}
}