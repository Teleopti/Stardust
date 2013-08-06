using System.Collections;
using System.Collections.Generic;

namespace Teleopti.Interfaces
{
    /// <summary>
    /// Interface for MAPI class that handles email creation
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2009-09-23
    /// </remarks>
    public interface IMapiMailMessage {
        /// <summary>
        /// Gets or sets the Subject of this mail message.
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of this mail message.
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// Gets the recipient list for this mail message.
        /// </summary>
        IList Recipients { get; }

        /// <summary>
        /// Gets the file list for this mail message.
        /// </summary>
        ArrayList Files { get; }

        /// <summary>
        /// Displays the mail message dialog asynchronously.
        /// </summary>
        void ShowDialog();

        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="filePaths">The file path.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-09-24
        /// </remarks>
        void CreateMessage(string address, ArrayList filePaths);
    }
}