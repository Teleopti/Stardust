using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages
{
	public class QuickForecastWizardPages : AbstractWizardPagesNoRoot<QuickForecastCommandDto>
	 {
		private readonly QuickForecastCommandDto _stateObj;

		public QuickForecastWizardPages(QuickForecastCommandDto stateObj)
			: base(stateObj)
		  {
				_stateObj = stateObj;
		  }

		public override QuickForecastCommandDto CreateNewStateObj()
		  {
				return _stateObj;
		  }

		public QuickForecastWorkloadsMessage CreateServiceBusMessage()
		{
			return new QuickForecastWorkloadsMessage
		{
			StatisticPeriod = _stateObj.StatisticPeriod.ToDateOnlyPeriod(),
			TargetPeriod = _stateObj.TargetPeriod.ToDateOnlyPeriod(),
			ScenarioId = _stateObj.ScenarioId,
			SmoothingStyle = _stateObj.SmoothingStyle,
			TemplatePeriod = _stateObj.TemplatePeriod.ToDateOnlyPeriod(),
			WorkloadIds = _stateObj.WorkloadIds,
			IncreaseWith = _stateObj.IncreaseWith,
			UseDayOfMonth = _stateObj.UseDayOfMonth
		};
		}
			
		  public override string Name
		  {
				get { return Resources.QuickForecast; }
		  }

		  public override string WindowText
		  {
			get { return Resources.QuickForecast; }
		  }
	 }
	public static class DateOnlyDtoExtensions
	{
		public static DateOnly ToDateOnly(this DateOnlyDto dateOnlyDto)
		{
			return new DateOnly(dateOnlyDto.DateTime);
		}

		public static DateOnly? ToNullableDateOnly(this DateOnlyDto dateOnlyDto)
		{
			return dateOnlyDto != null ? dateOnlyDto.ToDateOnly() : null;
		}

		public static DateOnlyPeriod ToDateOnlyPeriod(this DateOnlyPeriodDto dateOnlyPeriodDto)
		{
			return new DateOnlyPeriod(dateOnlyPeriodDto.StartDate.ToDateOnly(), dateOnlyPeriodDto.EndDate.ToDateOnly());
		}
	}
}
