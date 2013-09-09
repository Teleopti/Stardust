using System;

namespace Teleopti.Interfaces.Domain

{
    /// <summary>
    /// Interface for SlpsService
    /// </summary>
    public interface ISlpsService : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether CCCBase option is enabled.
        /// </summary>
        /// <value><c>true</c> if CCCBase is enabled; otherwise, <c>false</c>.</value>
        bool CccBaseEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether CCCAsm option is enabled.
        /// </summary>
        /// <value><c>true</c> if CCCAsm is enabled; otherwise, <c>false</c>.</value>
        bool CccAsmEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether RaptorBase option is enabled.
        /// </summary>
        /// <value><c>true</c> if RaptorBase is enabled; otherwise, <c>false</c>.</value>
        bool RaptorBaseEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether RaptorAss option is enabled.
        /// </summary>
        /// <value><c>true</c> if RaptorAss is enabled; otherwise, <c>false</c>.</value>
        bool RaptorAssEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether FreemiumBase option is enabled.
        /// </summary>
        /// <value><c>true</c> if FreemiumBase is enabled; otherwise, <c>false</c>.</value>
        bool FreemiumBaseEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether EarlyBirdBase option is enabled.
        /// </summary>
        /// <value><c>true</c> if EarlyBirdBase is enabled; otherwise, <c>false</c>.</value>
        bool EarlyBirdBaseEnabled { get; }
    }
}