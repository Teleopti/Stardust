﻿using System;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Display row for find results
    /// </summary>
    public interface IPersonFinderDisplayRow : IPersonAuthorization
    {
        /// <summary>
        /// First name
        /// </summary>
        string FirstName { get; set; }
        /// <summary>
        /// Last name
        /// </summary>
        string LastName { get; set; }
        /// <summary>
        /// Employment number
        /// </summary>
        string EmploymentNumber { get; set; }
        /// <summary>
        /// Note
        /// </summary>
        string Note { get; set; }
        /// <summary>
        /// Terminal date
        /// </summary>
        DateTime TerminalDate { get; set; }
        ///<summary>
        ///</summary>
        bool Grayed { get; set; }
        ///<summary>
        ///</summary>
        int TotalCount { get; set; }
        ///<summary>
        ///</summary>
        Int64 RowNumber { get; set; }
    }
}
