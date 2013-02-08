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
    }
}
