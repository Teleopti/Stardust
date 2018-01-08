using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	/// <summary>
	/// A class for BadgeSetting.
	/// It's designed to apply different threshold value types (count, percent, Timespan, etc.),
	/// A BadgeUnitType property indicates what data type it applied.
	/// All threshold values are converted into int to stored in this class.
	///   - For count, the value will be keep as it's original value
	///   - For percent, the value will be original value times with 10000 and round into int
	///   - For Timespan, the value will be seconds of the Timespan
	/// So when use the threshold value, you need check the UnitType property, and convert the value
	/// into original value with the static methods.
	/// 
	/// -- This is really not a good design, but I did not find a better solution :-(
	/// </summary>
	public class BadgeSetting : AggregateEntity, IBadgeSetting
	{
		private string _name;
		private int _qualityId;
		private bool _enabled;
		private bool _largerIsBetter;
		private ExternalPerformanceDataType _dataType;
		private int _threshold;
		private int _bronzeThreshold;
		private int _silverThreshold;
		private int _goldThreshold;

		public virtual string Name
		{
			get => _name;
			set => _name = value;
		}

		public virtual int QualityId
		{
			get => _qualityId;
			set => _qualityId = value;
		}

		public virtual bool Enabled
		{
			get => _enabled;
			set => _enabled = value;
		}

		public virtual bool LargerIsBetter
		{
			get => _largerIsBetter;
			set => _largerIsBetter = value;
		}

		public virtual ExternalPerformanceDataType DataType
		{
			get => _dataType;
			set => _dataType = value;
		}

		public virtual int Threshold
		{
			get => _threshold;
			set => _threshold = value;
		}

		public virtual int BronzeThreshold
		{
			get => _bronzeThreshold;
			set => _bronzeThreshold = value;
		}

		public virtual int SilverThreshold
		{
			get => _silverThreshold;
			set => _silverThreshold = value;
		}

		public virtual int GoldThreshold
		{
			get => _goldThreshold;
			set => _goldThreshold = value;
		}
	}
}