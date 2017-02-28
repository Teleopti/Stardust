using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public abstract class SkillType : VersionedAggregateRoot, ISkillType, IDeleteTag
    {
        private Description _description;
        private ForecastSource _forecastSource;
        private bool _isDeleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillType"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="forecastSource">The forecast source.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        protected SkillType(Description description, ForecastSource forecastSource)
        {
            _description = description;
            _forecastSource = forecastSource;
        }

        /// <summary>
        /// For NHibernate
        /// </summary>
        protected SkillType()
        {
        }

        /// <summary>
        /// Gets the default resolution.
        /// </summary>
        /// <value>The default resolution.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        public abstract int DefaultResolution{ get;}

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-25
        /// </remarks>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets the forecast source.
        /// </summary>
        /// <value>The forecast source.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-08-26
        /// </remarks>
        public virtual ForecastSource ForecastSource
        {
            get { return _forecastSource; }
            set { _forecastSource = value; }
        }

        public abstract ITaskTimeDistributionService TaskTimeDistributionService { get; }

        public abstract bool DisplayTimeSpanAsMinutes
        {get;}

		public abstract IStaffingCalculatorServiceFacade StaffingCalculatorService
        { get; set; }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
