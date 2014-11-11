using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Messages.General
{
    /// <summary>
    /// Messages contains details of performing import forecasts into one day
    /// </summary>
    public class ImportForecastsToSkill : MessageWithLogOnInfo
    {
        ///<summary>
        /// The Id of the job this message will feed with updates.
        ///</summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// The identity of this message.
        /// </summary>
        public override Guid Identity
        {
            get { return JobId; }
        }

        /// <summary>
        /// The target skill to be imported to
        /// </summary>
        public Guid TargetSkillId { get; set; }

        /// <summary>
        /// The owner of this action.
        /// </summary>
        public Guid OwnerPersonId { get; set; }

        /// <summary>
        /// The import date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Forecasts
        /// </summary>
        public ICollection<IForecastsRow> Forecasts { get; set; }

        /// <summary>
        /// Mode of importing forecasts
        /// </summary>
        public ImportForecastsMode ImportMode { get; set; }
    }
}
