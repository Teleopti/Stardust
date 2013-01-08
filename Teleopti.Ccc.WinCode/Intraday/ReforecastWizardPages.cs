using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class ReforecastWizardPages : AbstractWizardPagesNoRoot<ReforecastModelCollection>
    {
        private readonly ReforecastModelCollection _stateObj;

        public ReforecastWizardPages(ReforecastModelCollection stateObj) : base(stateObj)
        {
            _stateObj = stateObj;
        }

        public override ReforecastModelCollection CreateNewStateObj()
        {
            return _stateObj;
        }

        public override string Name
        {
            get { return UserTexts.Resources.Reforecast; }
        }

        public override string WindowText
        {
            get { return UserTexts.Resources.Reforecast; }
        }
    }
}
