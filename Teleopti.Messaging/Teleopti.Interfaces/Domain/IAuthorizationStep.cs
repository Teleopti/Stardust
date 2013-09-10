using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for the authorization steps.
    /// </summary>
    /// <remarks>
    /// The AuthorizationStep objects are the building steps
    /// for the authorization system.
    /// </remarks>
    public interface IAuthorizationStep
    {
        /// <summary>
        /// Gets the general list
        /// </summary>
        /// <returns></returns>
        IList<T> ProvidedList<T>() where T : IAuthorizationEntity;

        /// <summary>
        /// Re-read list from source
        /// </summary>
        void RefreshList();

        /// <summary>
        /// Gets the parents
        /// </summary>
        IList<IAuthorizationStep> Parents
        { get; }

        /// <summary>
        /// Get or sets the name of the panel
        /// </summary>
        string PanelName
        { get; }

        /// <summary>
        /// Get or sets the description of what the panel does
        /// </summary>
        string PanelDescription
        { get;}

        /// <summary>
        /// Gets the inner exception occurred while RefreshList() method.
        /// </summary>
        /// <value>The inner exception.</value>
        Exception InnerException
        { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IAuthorizationStep"/> is enabled by the user.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled
        { get; set; }

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        /// <value>The warning message.</value>
        string WarningMessage
        { get; set; }

    }
}