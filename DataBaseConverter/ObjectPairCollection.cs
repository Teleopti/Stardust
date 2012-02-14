using System.Collections;
using System.Collections.Generic;

namespace Teleopti.Ccc.DatabaseConverter
{
    /// <summary>
    /// A IList containing connected objects
    /// </summary>
    /// <typeparam name="TOld"></typeparam>
    /// <typeparam name="TNew"></typeparam>
    public class ObjectPairCollection<TOld, TNew> : IEnumerable<ObjectPair<TOld, TNew>> 
    {
        private readonly IList<ObjectPair<TOld, TNew>> _pairCollection = new List<ObjectPair<TOld, TNew>>();

        /// <summary>
        /// Adds the connected objects.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        public void Add(TOld obj1, TNew obj2)
        {
            _pairCollection.Add(new ObjectPair<TOld, TNew>(obj1, obj2));
        }

        /// <summary>
        /// Gets the paired objects. Returns null if the old object is not found
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <returns></returns>
        public TNew GetPaired(TOld obj1)
        {
            if (obj1 != null)
            {
                foreach (ObjectPair<TOld, TNew> pair in _pairCollection)
                {
                    if (pair.Obj1.Equals(obj1))
                    {
                        return pair.Obj2;
                    }
                }
            }
            return default(TNew);
        }

        /// <summary>
        /// Gets the obj2 collection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/30/2007
        /// </remarks>
        public ICollection<TNew> Obj2Collection()
        {
            ICollection<TNew> retList = new List<TNew>();
            foreach (ObjectPair<TOld, TNew> pair in _pairCollection)
            {
                retList.Add(pair.Obj2);
            }
            return retList;
        }

        /// <summary>
        /// Gets the obj1 collection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/30/2007
        /// </remarks>
        public ICollection<TOld> Obj1Collection()
        {
            ICollection<TOld> retList = new List<TOld>();
            foreach (ObjectPair<TOld, TNew> pair in _pairCollection)
            {
                retList.Add(pair.Obj1);
            }
            return retList;
        }

        #region IEnumerable<ObjectPair<TOld,TNew>> Members

        ///<summary>
        ///Returns an enumerator that iterates through the collection.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        IEnumerator<ObjectPair<TOld, TNew>> IEnumerable<ObjectPair<TOld, TNew>>.GetEnumerator()
        {
            return _pairCollection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        ///<summary>
        ///Returns an enumerator that iterates through a collection.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            return _pairCollection.GetEnumerator();
        }

        #endregion
    }
}