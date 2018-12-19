using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class AttemptsAttribute : Attribute
	{
		public int Attempts { get; set; }

		public AttemptsAttribute(int attempts)
		{
			Attempts = attempts;
		}
	}
}