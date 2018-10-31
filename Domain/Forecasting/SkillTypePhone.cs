using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Skill type for phone skills
    /// </summary>
    public class SkillTypePhone : SkillType, ISkillTypePhone
    {

        private readonly ITaskTimeDistributionService _service = new TaskTimePhoneDistributionService();
		private IStaffingCalculatorServiceFacade _staffingCalculatorService = new StaffingCalculatorServiceFacade();

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTypePhone"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        protected SkillTypePhone(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTypePhone"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="forecastSource">The forecast source.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        public SkillTypePhone(Description description, ForecastSource forecastSource)
            : base(description, forecastSource)
        {
        }

        public override int DefaultResolution => 15;

		public override ITaskTimeDistributionService TaskTimeDistributionService => _service;

		public override bool DisplayTimeSpanAsMinutes => false;

		public override IStaffingCalculatorServiceFacade StaffingCalculatorService
        {
			set { _staffingCalculatorService = value; }
            get { return _staffingCalculatorService; }
        }
    }
}