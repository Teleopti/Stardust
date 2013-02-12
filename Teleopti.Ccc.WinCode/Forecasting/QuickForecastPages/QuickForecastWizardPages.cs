using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages
{
	public class QuickForecastWizardPages : AbstractWizardPagesNoRoot<QuickForecastModel>
    {
		private readonly QuickForecastModel _stateObj;

		public QuickForecastWizardPages(QuickForecastModel stateObj)
			: base(stateObj)
        {
            _stateObj = stateObj;
        }

		public override QuickForecastModel CreateNewStateObj()
        {
            return _stateObj;
        }

        public override string Name
        {
            get { return "xxQuickForecast"; }
        }

        public override string WindowText
        {
			get { return "xxQuickForecast"; }
        }

		public QuickForecastCommandDto GetCommand()
		{
			// onödigt med två likadana modeller 
			 return new QuickForecastCommandDto
                {
                    ScenarioId = _stateObj.ScenarioId,
                    StatisticPeriod = _stateObj.StatisticPeriod ,
                    TargetPeriod = _stateObj.TargetPeriod,
                    UpdateStandardTemplates = _stateObj.UpdateStandardTemplates,
					TemplatePeriod = _stateObj.TemplatePeriod,
					SmoothingStyle = _stateObj.SmoothingStyle,
					WorkloadIds = _stateObj.SelectedWorkloads
                };
		}
    }
}
