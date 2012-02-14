using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for schedule that want to be classified by SignificantPart
    /// </summary>
    /// <remarks>
    /// Used for SignificantPartService to calculate the SignificantPart
    /// Created by: henrika
    /// Created date: 2009-02-17
    /// </remarks>
    public interface ISignificantPartProvider
    {
        /// <summary>
        /// Definition for HasDayOff
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-02-17
        /// </remarks>
        bool HasDayOff();

        /// <summary>
        /// Defitnition for HasFullAbsence
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-02-17
        /// </remarks>
        bool HasFullAbsence();

        /// <summary>
        /// Definition for HasMainShift
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-02-17
        /// </remarks>
        bool HasMainShift();

        /// <summary>
        /// Definition for HasAssignment
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-02-17
        /// </remarks>
        bool HasAssignment();

        /// <summary>
        /// Definition for HasPersonalshift
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-02-17
        /// </remarks>
        bool HasPersonalShift();
        /// <summary>
        /// Defenition for HasOvertimeShift
        /// </summary>
        /// <returns></returns>
        bool HasOvertimeShift();
        /// <summary>
        /// Determines whether this instance has any absence.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance has absence; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-05-26
        /// </remarks>
        bool HasAbsence();
        /// <summary>
        /// Definition for HasPreferenceRestriction
        /// </summary>
        /// <returns></returns>
        bool HasPreferenceRestriction();
        /// <summary>
        /// Definition for HasAvailabilityRestriction
        /// </summary>
        /// <returns></returns>
        bool HasStudentAvailabilityRestriction();

        ///<summary>
        ///</summary>
        ///<returns>True if no Schedule and Day Off according to Contract Schedule.
        /// True if Full Day Absence but Day Off according to Contract Schedule.
        /// Otherwise False</returns>
        bool HasContractDayOff();

        /// <summary>
        /// Begin a read action. Use dispose on the returned object to end the read. During a single read only one projection will be created and used.
        /// </summary>
        /// <returns></returns>
        IDisposable BeginRead();
    }
}
