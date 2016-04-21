using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class TrueFalseRandomizer : ITrueFalseRandomizer
	{
		private readonly Random random = new Random();

		public bool Randomize()
		{
			return random.NextDouble() > 0.5;
		}
	}
}