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
    public struct OpenForWork : IEquatable<OpenForWork>
    {
	    private readonly bool _isOpen;
	    private readonly bool _isOpenForIncomingWork;

	    public OpenForWork(bool isOpen, bool isOpenForIncomingWork)
		{
			_isOpen = isOpen;
			_isOpenForIncomingWork = isOpenForIncomingWork;
		}

	    /// <summary>
        /// Gets or sets that the day is open, Here we consider only open working hours.
        /// </summary>
        /// <value> ture = open working hours or day is open. false= close working hours or day is closed.</value>
        /// <remarks>
        /// Created by: talham
        /// Created date: 2012-02-28
        /// </remarks>
        public bool IsOpen
	    {
		    get { return _isOpen; }
	    }

	    /// <summary>
        /// Gets or sets that the day is open for incoming work such as Email, Fax anything other than telephony.
        /// </summary>
        /// <value> ture = open for incoming work. false= close for incoming work.</value>
        /// <remarks>
        /// Created by: talham
        /// Created date: 2012-02-28
        /// </remarks>
        public bool IsOpenForIncomingWork
	    {
		    get { return _isOpenForIncomingWork; }
	    }

	    public bool Equals(OpenForWork other)
	    {
		    return other._isOpen == IsOpen &&
		           other._isOpenForIncomingWork == _isOpenForIncomingWork;
	    }

		public override bool Equals(object obj)
		{
			if (obj is OpenForWork)
			{
				return Equals((OpenForWork) obj);
			}
			return false;
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
            return 317 ^ _isOpen.GetHashCode() ^ _isOpenForIncomingWork.GetHashCode();
        }

		/// <summary>
		/// Operator ==.
		/// </summary>
		/// <param name="per1">The per1.</param>
		/// <param name="per2">The per2.</param>
		/// <returns></returns>
		public static bool operator ==(OpenForWork per1, OpenForWork per2)
		{
			return per1.Equals(per2);
		}

		/// <summary>
		/// Operator !=.
		/// </summary>
		/// <param name="per1">The per1.</param>
		/// <param name="per2">The per2.</param>
		/// <returns></returns>
		public static bool operator !=(OpenForWork per1, OpenForWork per2)
		{
			return !per1.Equals(per2);
		}
    }
}