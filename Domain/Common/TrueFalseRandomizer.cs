using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class TrueFalseRandomizer : ITrueFalseRandomizer
	{
		 public bool Randomize(int seed)
		 {
			 var rnd = new Random(seed);
			 int falseCount = 0;
			 for (int i = 0; i < 100; i++)
			 {
				 int value = rnd.Next(0, 2);
				 if (value < 1)
					 falseCount++;
			 }

			 if (falseCount > 50)
				 return false;

			 return true;
		 }
	}

	public class FalseRandomizerForTest : ITrueFalseRandomizer
	{
		public bool Randomize(int seed)
		{
			return false;
		}
	}
}