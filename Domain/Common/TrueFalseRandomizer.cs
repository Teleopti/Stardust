using System;
using System.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class TrueFalseRandomizer : ITrueFalseRandomizer
	{
		 public bool Randomize(int seed)
		 {
		 	Random rnd = new Random(seed);
		 	int value = rnd.Next(0, 2);
			//if (value == 2)
			//{
			//    throw new ConstraintException("Value was 2");
			//}
		 	if (value == 0)
				return false;

		 	return true;
		 }
	}
}