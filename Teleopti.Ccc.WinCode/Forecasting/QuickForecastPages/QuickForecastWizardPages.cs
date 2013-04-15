using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;

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

        public override string Name
        {
            get { return Resources.QuickForecast; }
        }

        public override string WindowText
        {
			get { return Resources.QuickForecast; }
        }
    }
}
