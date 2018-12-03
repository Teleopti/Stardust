using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class Campaign : ICampaign
    {
        Percent _campaignTasksPercent;
        Percent _campaignTaskTimePercent;
        Percent _campaignAfterTaskTimePercent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Campaign"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public Campaign()
        {
            _campaignTasksPercent = new Percent();
            _campaignTaskTimePercent = new Percent();
            _campaignAfterTaskTimePercent = new Percent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Campaign"/> class.
        /// </summary>
        /// <param name="campaignTasksPercent">The campaign tasks percent.</param>
        /// <param name="campaignTaskTimePercent">The campaign task time percent.</param>
        /// <param name="campaignAfterTaskTimePercent">The campaign after task time percent.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public Campaign(Percent campaignTasksPercent,
            Percent campaignTaskTimePercent,
            Percent campaignAfterTaskTimePercent)
        {
            _campaignTasksPercent = campaignTasksPercent;
            _campaignTaskTimePercent = campaignTaskTimePercent;
            _campaignAfterTaskTimePercent = campaignAfterTaskTimePercent;
        }

        #region Properties

        /// <summary>
        /// Gets the campaign tasks percent.
        /// </summary>
        /// <value>The campaign tasks percent.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public Percent CampaignTasksPercent
        {
            get { return _campaignTasksPercent; }
        }

        /// <summary>
        /// Gets the campaign task time percent.
        /// </summary>
        /// <value>The campaign task time percent.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public Percent CampaignTaskTimePercent
        {
            get { return _campaignTaskTimePercent; }
        }

        /// <summary>
        /// Gets the campaign after task time percent.
        /// </summary>
        /// <value>The campaign after task time percent.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        public Percent CampaignAfterTaskTimePercent
        {
            get { return _campaignAfterTaskTimePercent; }
        }

        #endregion

        #region IEquatable<Campaign> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public bool Equals(ICampaign other)
        {
            if (other == null)
                return false;

            return other.CampaignAfterTaskTimePercent == _campaignAfterTaskTimePercent &&
                   other.CampaignTasksPercent == _campaignTasksPercent &&
                   other.CampaignTaskTimePercent == _campaignTaskTimePercent;

        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public override bool Equals(object obj)
		{
			return obj is Campaign campaign && Equals(campaign);
		}

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public override int GetHashCode()
        {
            return (string.Concat(GetType().FullName, "|",
                _campaignAfterTaskTimePercent, "|" ,
                _campaignTasksPercent, "|" ,
                _campaignTaskTimePercent)).GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="campaign1">The campaign1.</param>
        /// <param name="campaign2">The campaign2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public static bool operator ==(Campaign campaign1, Campaign campaign2)
        {
            if ((object)campaign1 == null)
                return false;

            if ((object)campaign2 == null)
                return false;

            return campaign1.Equals(campaign2);
        }


        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="campaign1">The campaign1.</param>
        /// <param name="campaign2">The campaign2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public static bool operator !=(Campaign campaign1, Campaign campaign2)
        {
            if ((object)campaign1 == null && (object)campaign2 == null)
                return false;

            if ((object)campaign1 == null)
                return true;

            if ((object)campaign2 == null)
                return true;

            return !campaign1.Equals(campaign2);
        }

        #endregion
    }
}
