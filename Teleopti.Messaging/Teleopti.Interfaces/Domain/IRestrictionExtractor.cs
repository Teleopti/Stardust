﻿using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{

	/// <summary>
	/// 
	/// </summary>
	public interface IRestrictionExtractor : IRestrictionExtractorWithoutStateHolder
	{

		/// <summary>
		/// Extracts the specified person.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="dateOnly">The date only.</param>
		void Extract(IPerson person, DateOnly dateOnly);
	}

    /// <summary>
    /// 
    /// </summary>
    public interface IRestrictionExtractorWithoutStateHolder
    {
        /// <summary>
        /// Gets the availability list.
        /// </summary>
        /// <value>The availability list.</value>
        IEnumerable<IAvailabilityRestriction> AvailabilityList { get; }

        /// <summary>
        /// Gets the rotation list.
        /// </summary>
        /// <value>The rotation list.</value>
        IEnumerable<IRotationRestriction> RotationList { get; }

        /// <summary>
        /// Gets the student availability list.
        /// </summary>
        /// <value>The student availability list.</value>
        IEnumerable<IStudentAvailabilityDay> StudentAvailabilityList { get; }

        /// <summary>
        /// Gets the preference list.
        /// </summary>
        /// <value>The preference list.</value>
        IEnumerable<IPreferenceRestriction> PreferenceList { get; }

        /// <summary>
        /// Extracts the specified schedule part.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        void Extract(IScheduleDay schedulePart);

        /// <summary>
        /// Combineds the restriction.
        /// </summary>
        /// <param name="schedulingOptions">The scheduling options.</param>
        /// <returns></returns>
        IEffectiveRestriction CombinedRestriction(ISchedulingOptions schedulingOptions);
    }
}
