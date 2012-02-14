using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Ccc.AgentPortalCode.Helper;

namespace Teleopti.Ccc.AgentPortal.Requests
{

    /// <summary>
    /// Represents a class that hold the state of the Request screen.
    /// </summary>
    public class RequestStateHolder
    {
        private int _resolution=30;

        /// <summary>
        /// Gets or sets the Time interval in Time selection dropdwon.
        /// </summary>
        /// <value>The resolution.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-22
        /// </remarks>
        public int Resolution
        {
            get { return _resolution; }
            set { _resolution = value; }
        }

        /// <summary>
        /// Gets the absence.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-06-25
        /// </remarks>
        public static  IList<AbsenceDto> LoadRequestableAbsence()
        {
            return
                SdkServiceHelper.SchedulingService.GetAbsences(new AbsenceLoadOptionDto
                                                                   {
                                                                       LoadRequestable = true,
                                                                       LoadRequestableSpecified = true
                                                                   });
        }
    }
}
