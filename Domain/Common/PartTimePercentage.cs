using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Percent=Teleopti.Interfaces.Domain.Percent;

namespace Teleopti.Ccc.Domain.Common
{

    public class PartTimePercentage : VersionedAggregateRootWithBusinessUnit, IPartTimePercentage, IDeleteTag
    {
        private Description _description;
        private Percent _percentage;
        private bool _isDeleted;

        /// <summary>
        /// Creates a new instance of PartTimePercentage
        /// </summary>
        /// <param name="name">Name of PartTimePercentage</param>
        public PartTimePercentage(string name)
        {
            _percentage = new Percent(1d);
            _description = new Description(name);
        }

        /// <summary>
        /// Constructor for NHibernate
        /// </summary>
        protected PartTimePercentage()
        {
        }

        /// <summary>
        /// Description of PartTimePercentage
        /// </summary>
        public virtual Description Description
        {
            get { return _description; }
            set{ _description = value;}
        }

        /// <summary>
        /// Percentage of full time
        /// </summary>
        public virtual Percent Percentage
        {
            get { return _percentage; }
            set
            {
                InParameter.BetweenZeroAndHundredPercent(nameof(Percentage), value);
                _percentage = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        public virtual bool IsChoosable
        {
            get { return !IsDeleted;}
        }

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