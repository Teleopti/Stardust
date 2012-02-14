using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class ResourceOptimizerAdvancedPreferencesPanel : BaseUserControl, IDataExchange
    {

        #region Variables
        
        private IOptimizerAdvancedPreferences _performanceAdvancedPreferences;

        #endregion

        #region Constructor and initialize

        public ResourceOptimizerAdvancedPreferencesPanel()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        /// <summary>
        /// Initializes the reoptimizer preferences data. Note: As a rule you call this method it before you start using the component.
        /// </summary>
        /// <param name="optimizerAdvancedPreferences">The optimizer criteria preferences.</param>
        public void Initialize(IOptimizerAdvancedPreferences optimizerAdvancedPreferences)
        {
            OptimizerAdvancedPreferences = optimizerAdvancedPreferences;
            ExchangeData(ExchangeDataOption.DataSourceToControls);
        }

        #endregion

        #region Interface

        /// <summary>
        /// Gets or sets the re optimizer preferences.
        /// </summary>
        /// <value>The re optimizer preferences.</value>
        public IOptimizerAdvancedPreferences OptimizerAdvancedPreferences
        {
            get { return _performanceAdvancedPreferences; }
            set { _performanceAdvancedPreferences = value; }
        }

        #region IDataExchange Members

        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
        }

        public void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.DataSourceToControls)
            {
                checkBoxUseMaxStddev.Checked = OptimizerAdvancedPreferences.ConsiderMaximumIntraIntervalStandardDeviation;
                checkBoxAdvUseTweakedValues.Checked = OptimizerAdvancedPreferences.UseTweakedValues;
                radioButtonUseRMS.Checked = true;
                radioButtonTeleopti.Checked = OptimizerAdvancedPreferences.UseTeleoptiCalculation;
                radioButtonUseStandardDeviation.Checked = OptimizerAdvancedPreferences.UseStandardDeviationCalculation;
            }
            else
            {
                OptimizerAdvancedPreferences.ConsiderMaximumIntraIntervalStandardDeviation = checkBoxUseMaxStddev.Checked;
                OptimizerAdvancedPreferences.UseTweakedValues = checkBoxAdvUseTweakedValues.Checked;
                OptimizerAdvancedPreferences.UseTeleoptiCalculation = radioButtonTeleopti.Checked;
                OptimizerAdvancedPreferences.UseStandardDeviationCalculation = radioButtonUseStandardDeviation.Checked;
            }
        }

        #endregion

        #endregion

    }
}