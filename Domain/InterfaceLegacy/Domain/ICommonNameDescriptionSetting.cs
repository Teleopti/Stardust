﻿namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Settings for how name of persons should be shown
    ///</summary>
    public interface ICommonNameDescriptionSetting
    {
        /// <summary>
        /// Builds the common name description.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-07-07
        /// </remarks>
        string BuildFor(IPerson person);

        ///<summary>
        ///</summary>
        ///<param name="lightPerson"></param>
        ///<returns></returns>
        string BuildFor(ILightPerson lightPerson);

	    /// <summary>
	    /// </summary>
	    /// <param name="firstName"></param>
	    /// <param name="lastName"></param>
	    /// <param name="employmentNumber"></param>
	    /// <returns></returns>
	    string BuildFor(string firstName, string lastName, string employmentNumber);
    }
}