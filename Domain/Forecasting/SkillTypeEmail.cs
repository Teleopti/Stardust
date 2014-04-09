using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Skill type for e-mail skills
    /// </summary>
    public class SkillTypeEmail : SkillType, ISkillTypeEmail
    {
        private readonly ITaskTimeDistributionService _service = new TaskTimeEmailDistributionService();

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTypeEmail"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        protected SkillTypeEmail (){}

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTypeEmail"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="forecastSource">The forecast source.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        public SkillTypeEmail(Description description, ForecastSource forecastSource)
            : base(description, forecastSource)
        {
        }

        public override int DefaultResolution
        {
            get
            {
                return 60;
            }
        }

        public override ITaskTimeDistributionService TaskTimeDistributionService
        {
            get { return _service; }
        }

        public override bool DisplayTimeSpanAsMinutes
        {
            get { return true; }
        }

        public override IStaffingCalculatorService StaffingCalculatorService
        {
            get { return new StaffingEmailCalculatorService(); }
        }
    }
}
