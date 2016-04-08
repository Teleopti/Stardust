using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds information of what has been changed between two items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-29
    /// </remarks>
    public struct DifferenceCollectionItem<T> where T : class 
    {
        private readonly T _originalItem;
        private readonly T _currentItem;
        private DifferenceStatus? _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceCollectionItem{T}"/> struct.
        /// </summary>
        /// <param name="originalItem">The original item.</param>
        /// <param name="currentItem">The current item.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-29
        /// </remarks>
        public DifferenceCollectionItem(T originalItem, T currentItem)
        {
            if(originalItem==null && currentItem==null)
                throw new ArgumentException("Both originalItem and currentItem must not be null");
            _originalItem = originalItem;
            _currentItem = currentItem;
            _status = null;
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-29
        /// </remarks>
        public DifferenceStatus Status
        {
            get
            {
                if (!_status.HasValue)
                    setStatus();
                return _status.Value;
            }
        }

        private void setStatus()
        {
            _status = DifferenceStatus.Modified;
            if (OriginalItem == null)
                _status = DifferenceStatus.Added;
            if (CurrentItem == null)
                _status = DifferenceStatus.Deleted;
        }

        /// <summary>
        /// Gets the original item.
        /// </summary>
        /// <value>The original item.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-29
        /// </remarks>
        public T OriginalItem
        {
            get { return _originalItem; }
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <value>The current item.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-29
        /// </remarks>
        public T CurrentItem
        {
            get { return _currentItem; }
        }

        #region Equals stuff

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(DifferenceCollectionItem<T> other)
        {
            if(other.Status.Equals(Status))
            {
                if(CurrentItem == null)
                {
                    if(other.CurrentItem != null)
                        return false;
                }
                else
                {
                    if(!CurrentItem.Equals(other.CurrentItem))
                        return false;
                }
                if (OriginalItem == null)
                {
                    if (other.OriginalItem != null)
                        return false;
                }
                else
                {
                    if(!OriginalItem.Equals(other.OriginalItem))
                        return false;
                }
            }
            else
            {
                return false;
            }
            return true;
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
            if (obj == null || !(obj is DifferenceCollectionItem<T>))
            {
                return false;
            }
            return Equals((DifferenceCollectionItem<T>)obj);
        }

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="obj1">The per1.</param>
        /// <param name="obj2">The per2.</param>
        /// <returns></returns>
        public static bool operator ==(DifferenceCollectionItem<T> obj1, DifferenceCollectionItem<T> obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="obj1">The per1.</param>
        /// <param name="obj2">The per2.</param>
        /// <returns></returns>
        public static bool operator !=(DifferenceCollectionItem<T> obj1, DifferenceCollectionItem<T> obj2)
        {
            return !obj1.Equals(obj2);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            int res = Status.GetHashCode();
            if (CurrentItem != null)
                res = res ^ CurrentItem.GetHashCode();
            if (OriginalItem != null)
                res = res ^ OriginalItem.GetHashCode();
            return res;
        }

        #endregion
    }
}