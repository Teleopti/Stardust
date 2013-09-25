using System;
using System.Collections.Generic;
using System.Data;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
		[IsNotDeadCode("Used in NH mapping files.")]
    public class TrackerByte:IUserType
    {
        private static readonly IList<ITracker> _trackers = Tracker.AllTrackers();
        private static readonly SqlType[] _types = { new SqlType(DbType.Byte) };

        public new bool Equals(object x, object y)
        {
            bool returnvalue = false;
            if (x == null && y == null)
                returnvalue = true;
            else if ((x != null))
            {
                returnvalue = x.Equals(y);
            }
            return returnvalue;
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            var b = NHibernateUtil.Byte.NullSafeGet(rs, names[0]) as byte?;
            if (b.HasValue)
                if (b.Value < _trackers.Count)
                    return _trackers[b.Value];

            return null;   
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            int indexOfTrackerInstance = _trackers.IndexOf((ITracker) value);
            NHibernateUtil.Byte.NullSafeSet(cmd, (byte)indexOfTrackerInstance, index);
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public SqlType[] SqlTypes
        {
            get
            {
                return _types;
            }
        }

        public Type ReturnedType
        {
            get { return typeof (Tracker); }
        }

        public bool IsMutable
        {
            get { return false; }
        }

        #region NotImplemented
        /// <summary> 
        /// During merge, replace the existing (target) value in the entity we are merging to with a new (original) value from the detached entity we are merging.     ''' </summary>    ''' <param name="original">the value from the detached entity being merged</param>    
        /// <param name="target">the value in the managed entity</param>    
        /// <param name="owner">the managed entity</param>    
        /// <returns>the value to be merged</returns>    
        /// <remarks>For immutable objects, or null values, it is safe to simply return the first parameter.     
        /// For mutable objects, it is safe to return a copy of the first parameter.     
        /// For objects with component values, it might make sense to recursively replace component values.</remarks>
        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        ///<summary>    
        ///Reconstruct an object from the cacheable representation.     
        ///</summary>    
        ///<param name="cached"></param>    
        /// <param name="owner"></param>    
        /// <returns></returns>    
        /// <remarks>At the very least this method should perform a deep copy if the type is mutable. (optional operation)</remarks>

        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        ///<summary>    
        /// Transform the object into its cacheable representation.     
        /// </summary>    
        /// <param name="value"></param>    
        /// <returns></returns>    
        /// <remarks>At the very least this method should perform a deep copy if the type is mutable.     
        /// That may not be enough for some implementations, however; for example, associations must be cached as identifier values. (optional operation)
        /// </remarks>
        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        ///<summary>    
        /// Get a hashcode for the instance, consistent with persistence "equality"    
        /// </summary>    
        /// <param name="x"></param>    
        /// <returns></returns>    
        /// <remarks></remarks>
        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }
        #endregion //NotImplemented
    }
}