using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class that implements properties that the day is open for Incoming work or not.
    /// </summary>
    /// <remarks>
    /// Created by: Talham
    /// Created date: 2012-02-28
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals")]
    public class OpenForWork : IOpenForWork
    {
        /// <summary>
        /// Gets or sets that the day is open, Here we consider only open working hours.
        /// </summary>
        /// <value> ture = open working hours or day is open. false= close working hours or day is closed.</value>
        /// <remarks>
        /// Created by: talham
        /// Created date: 2012-02-28
        /// </remarks>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Gets or sets that the day is open for incoming work such as Email, Fax anything other than telephony.
        /// </summary>
        /// <value> ture = open for incoming work. false= close for incoming work.</value>
        /// <remarks>
        /// Created by: talham
        /// Created date: 2012-02-28
        /// </remarks>
        public bool IsOpenForIncomingWork { get; set; }


        /// <summary>
        /// Compare object Instances..
        /// </summary>
        /// <remarks>
        /// Created by: talham
        /// Created date: 2012-02-28
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override bool Equals(object obj)
        {
            var open = obj as OpenForWork;
            if(open == null)
                throw new ArgumentException("Input parameter should be instance of OpenForWork");
            return open.IsOpen == IsOpen && open.IsOpenForIncomingWork == IsOpenForIncomingWork;
        }

        /// <summary>
        /// Overrides GetHashCode function.
        /// </summary>
        /// <remarks>
        /// Created by: talham
        /// Created date: 2012-02-28
        /// </remarks>
        public override int GetHashCode()
        {
            // ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
            // ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
        }
    }
}