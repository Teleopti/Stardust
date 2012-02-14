#region Imports

using System;

#endregion

namespace Teleopti.Ccc.AgentPortal.Proxy
{

    /// <summary>
    /// Represents a discovery protocol
    /// </summary>
    [Flags]
    [Serializable]
    public enum Protocols
    {
        /// <summary>
        /// Soap
        /// </summary>
        Soap,
        /// <summary>
        /// Soap12
        /// </summary>
        Soap12,
        /// <summary>
        /// Http Get
        /// </summary>
        HttpGet,
        /// <summary>
        /// Http Post
        /// </summary>
        HttpPost,
        /// <summary>
        /// Http Soap
        /// </summary>
        HttpSoap
    }
}
