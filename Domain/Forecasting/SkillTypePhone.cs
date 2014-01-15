using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Skill type for phone skills
    /// </summary>
    public class SkillTypePhone : SkillType, ISkillTypePhone
    {

        private readonly ITaskTimeDistributionService _service = new TaskTimePhoneDistributionService();
        private readonly IStaffingCalculatorService _staffingCalculatorService = new StaffingCalculatorService();

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

        public override int DefaultResolution
        {
            get { return 15; }
        }

        public override ITaskTimeDistributionService TaskTimeDistributionService
        {
            get { return _service; }
        }

        public override bool DisplayTimeSpanAsMinutes
        {
            get { return false; }
        }

        public override IStaffingCalculatorService StaffingCalculatorService
        {
            get { return _staffingCalculatorService; }
        }
    }
}