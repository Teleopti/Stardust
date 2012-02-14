using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// IMyReportScheduleInfo interface
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 11/24/2008
    /// </remarks>
    public interface IMyReportScheduleInfo
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        DateTime ScheduleDate { get; }

        /// <summary>
        /// Gets or sets the adherence.
        /// </summary>
        /// <value>The adherence.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        double Adherence { get; }

        /// <summary>
        /// Gets or sets the payload info.
        /// </summary>
        /// <value>The payload info.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        ReadOnlyCollection<IMyReportVisualPayloadInfo> PayloadInfo { get; }

        /// <summary>
        /// Gets or sets the adherence info.
        /// </summary>
        /// <value>The adherence info.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        ReadOnlyCollection<IMyReportAdherenceInfo> AdherenceInfo { get; }

        /// <summary>
        /// Adds the payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void AddPayload(IMyReportVisualPayloadInfo payload);

        /// <summary>
        /// Removes the payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void RemovePayload(IMyReportVisualPayloadInfo payload);

        /// <summary>
        /// Removes all payloads.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void RemoveAllPayloads();

        /// <summary>
        /// Adds the adherence.
        /// </summary>
        /// <param name="adherence">The adherence.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void AddAdherence(IMyReportAdherenceInfo adherence);

        /// <summary>
        /// Removes the adherence.
        /// </summary>
        /// <param name="adherence">The adherence.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void RemoveAdherence(IMyReportAdherenceInfo adherence);

        /// <summary>
        /// Removes all adherences.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/25/2008
        /// </remarks>
        void RemoveAllAdherences();
    }
}
