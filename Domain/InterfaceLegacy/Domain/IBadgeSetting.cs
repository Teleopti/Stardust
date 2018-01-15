using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IBadgeSetting : IAggregateEntity
	{
		string Name { get; set; }
		int QualityId { get; set; }
		bool LargerIsBetter { get; set; }

		bool Enabled { get; set; }

		double Threshold { get; set; }
		double BronzeThreshold { get; set; }
		double SilverThreshold { get; set; }
		double GoldThreshold { get; set; }

		ExternalPerformanceDataType DataType { get; set; }
	}
}