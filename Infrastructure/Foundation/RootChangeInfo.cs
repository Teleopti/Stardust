using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Holds info about what and how a root has been persisted
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-06-12
    /// </remarks>
    public struct RootChangeInfo : IRootChangeInfo
    {
        private readonly IAggregateRoot _root;
        private readonly DomainUpdateType _status;

        public RootChangeInfo(IAggregateRoot aggregateRoot, DomainUpdateType status)
        {
            _root = aggregateRoot;
            _status = status;
        }

        public DomainUpdateType Status
        {
            get { return _status; }
        }

        public object Root
        {
            get { return _root; }
        }


        #region Equals stuff

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(RootChangeInfo other)
        {
            return other.Status == Status && other.Root.Equals(Root);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if obj and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is RootChangeInfo))
            {
                return false;
            }
            return Equals((RootChangeInfo)obj);
        }

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator ==(RootChangeInfo per1, RootChangeInfo per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator !=(RootChangeInfo per1, RootChangeInfo per2)
        {
            return !per1.Equals(per2);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return _root.Id.GetHashCode() ^ _status.GetHashCode();
        }

        #endregion
    }
}
