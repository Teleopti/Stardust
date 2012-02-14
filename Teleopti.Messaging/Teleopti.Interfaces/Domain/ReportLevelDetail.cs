
using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Tells if an activity is considered as a short break, lunch or none
    /// </summary>
    [Serializable]
    public enum ReportLevelDetail
    {
        /// <summary>
        /// Activity is not lunch or short break
        /// </summary>
        None,
        /// <summary>
        /// Activity is Short Break
        /// </summary>
        ShortBreak,
        /// <summary>
        /// Activity is Lunch
        /// </summary>
        Lunch
    }
}
