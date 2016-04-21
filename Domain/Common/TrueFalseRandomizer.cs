using System;
using System.Runtime.CompilerServices;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class TrueFalseRandomizer : ITrueFalseRandomizer
	{
		private static readonly Random random = new Random();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool Randomize()
		{
			return random.NextDouble() > 0.5;
		}
	}
}