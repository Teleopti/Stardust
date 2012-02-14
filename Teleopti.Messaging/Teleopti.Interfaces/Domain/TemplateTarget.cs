using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Type of target for templates
    /// </summary>
    [Serializable]
    public enum TemplateTarget
    {
        /// <summary>
        /// Skill
        /// </summary>
        Skill = 0,
        /// <summary>
        /// Workload
        /// </summary>
        Workload,
        /// <summary>
        /// Multisite
        /// </summary>
        Multisite
    }
}