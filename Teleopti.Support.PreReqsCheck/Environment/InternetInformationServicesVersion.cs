// ---------------------------------------------------------------------------
// Campari Software
//
// InternetInformationServicesVersion.cs
//
//
// ---------------------------------------------------------------------------
// Copyright (C) 2006-2007 Campari Software
// All rights reserved.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
// ---------------------------------------------------------------------------

namespace CheckPreRequisites.Environment
{
    #region enum InternetInformationServicesVersion
    /// <summary>
    /// Specifies the Internet Information Services (IIS) versions
    /// </summary>
    public enum InternetInformationServicesVersion
    {
        /// <summary>
        /// Internet Information Services 5.1
        /// </summary>
        /// <remarks>Shipped in Windows XP Professional</remarks>
        IIS51,

        /// <summary>
        /// Internet Information Services 6
        /// </summary>
        /// <remarks>Shipped in Windows Server 2003</remarks>
        IIS6,

        /// <summary>
        /// Internet Information Services 7
        /// </summary>
        /// <remarks>Shipped in Windows Vista</remarks>
        IIS7,
    }
    #endregion
}
