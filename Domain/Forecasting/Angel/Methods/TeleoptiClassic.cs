namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public abstract class TeleoptiClassic : TeleoptiClassicBase
	{
		protected TeleoptiClassic(IIndexVolumes indexVolumes)
			: base(indexVolumes)
		{
		}
	}

	public class TeleoptiClassicLongTermWithDayInMonth : TeleoptiClassic
	{
		public TeleoptiClassicLongTermWithDayInMonth() : base(new IndexVolumesLongTermWithDayInMonth())
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicLongTermWithDayInMonth; }
		}
	}

	public class TeleoptiClassicLongTerm : TeleoptiClassic
	{
		public TeleoptiClassicLongTerm() : base(new IndexVolumesLongTerm())
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicLongTerm; }
		}
	}

	public class TeleoptiClassicMediumTerm : TeleoptiClassic
	{
		public TeleoptiClassicMediumTerm() : base(new IndexVolumesMediumTerm())
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicMediumTerm; }
		}
	}

	public class TeleoptiClassicMediumTermWithDayInMonth : TeleoptiClassic
	{
		public TeleoptiClassicMediumTermWithDayInMonth() : base(new IndexVolumesLongTermWithDayInMonth())
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonth; }
		}
	}

	public class TeleoptiClassicShortTerm : TeleoptiClassic
	{

		public TeleoptiClassicShortTerm() : base(new IndexVolumesShortTerm())
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicShortTerm; }
 		}
 	}
}