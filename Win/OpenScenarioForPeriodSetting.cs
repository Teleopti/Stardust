using System;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Win.Common.Controls
{
    [Serializable]
    public class OpenScenarioForPeriodSetting : SettingValue
    {
        private Guid? _scenarioId;
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today.AddDays(1);
        //Negations because default values did not work as expected
        private bool _noShrinkage;
        private bool _noCalculation;
        private bool _noValidation;

		public OpenScenarioForPeriodSetting()
		{
			TeamLeaderMode = false;
		}

        public Guid? ScenarioId
        {
            get { return _scenarioId; }
            set { _scenarioId = value; }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        public bool NoShrinkage
        {
            get { return _noShrinkage; }
            set { _noShrinkage = value; }
        }

        public bool NoCalculation
        {
            get { return _noCalculation; }
            set { _noCalculation = value; }
        }

        public bool NoValidation
        {
            get { return _noValidation; }
            set { _noValidation = value; }
        }

    	public bool TeamLeaderMode { get; set; }
	    public bool NoRequests { get; set; }
    }
}