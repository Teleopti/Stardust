using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public static class BadgeType
	{
		public const int AnsweredCalls = 0;
		public const int AverageHandlingTime = 1;
		public const int Adherence = 2;

		public static int GetBadgeType(string type)
		{
			switch (type)
			{
				case nameof(Adherence):
					return Adherence;
				case nameof(AnsweredCalls):
					return AnsweredCalls;
				case nameof(AverageHandlingTime):
					return AverageHandlingTime;
				default:
					throw new ArgumentException(@"BadgeType", string.Format("\"{0}\" is not a valid badge type.", type));
			}
		}
	}
}