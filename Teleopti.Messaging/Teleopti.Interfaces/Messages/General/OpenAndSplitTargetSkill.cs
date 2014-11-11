using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Messages.General
{
    /// <summary>
    /// Message with details to perform an import of forecasting data to a target skill.
    /// </summary>
    public class OpenAndSplitTargetSkill : MessageWithLogOnInfo
    {
        ///<summary>
        /// The Id of the job this message will feed with updates.
        ///</summary>
        public Guid JobId { get; set; }
        
        ///<summary>
        ///  The Id of the job this message will feed with updates.
        ///</summary>
        public Guid JobResultId { get; set; }

        /// <summary>
        /// The identity of this message.
        /// </summary>
        public override Guid Identity
        {
            get { return JobId; }
        }

        ///<summary>
        /// The import date in the target skills time zone.
        ///</summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The owner of this action.
        /// </summary>
        public Guid OwnerPersonId { get; set; }
        
        /// <summary>
        /// The target skill Id.
        /// </summary>
        public Guid TargetSkillId { get; set; }

        /// <summary>
        /// Forecasts
        /// </summary>
        public ICollection<IForecastsRow> Forecasts { get; set; }

        /// <summary>
        /// Open hour start time
        /// </summary>
        public TimeSpan StartOpenHour { get; set; }  
        
        /// <summary>
        /// Open hour end time
        /// </summary>
        public TimeSpan EndOpenHour { get; set; }
        
        /// <summary>
        /// Mode of importing forecasts
        /// </summary>
        public ImportForecastsMode ImportMode { get; set; }
    }
}
