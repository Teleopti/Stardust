using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class Pair<T> : IPair<T>, IEquatable<IPair<T>>
    {
        public Pair(T first, T second)
        {
            First = first;
            Second = second;
        }

        public T First { get; private set; }

        public T Second { get; private set; }

        #region Implementation of IEquatable<T>

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
               return First.GetHashCode() ^ Second.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public bool Equals(IPair<T> other)
        {
            return (GetHashCode() == other.GetHashCode());
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            IPair<T> ent = obj as IPair<T>;
            if (ent == null)
                return false;

            return Equals(ent);
        }
        #endregion
    }
}
