﻿namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IExternalBadgeSetting : IAggregateEntity
	{
		string Name { get; set; }
		int QualityId { get; set; }
		bool LargerIsBetter { get; set; }

		bool Enabled { get; set; }

		int Threshold { get; set; }
		int BronzeThreshold { get; set; }
		int SilverThreshold { get; set; }
		int GoldThreshold { get; set; }

		BadgeUnitType UnitType { get; set; }
	}
}