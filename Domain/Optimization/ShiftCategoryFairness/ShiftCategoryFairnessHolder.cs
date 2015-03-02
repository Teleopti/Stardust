using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public class ShiftCategoryFairnessHolder : IShiftCategoryFairnessHolder
    {
        private IDictionary<IShiftCategory, int> _shiftDictionary;
    	private readonly IFairnessValueResult _fairnessValueResult;

    	public ShiftCategoryFairnessHolder()
        {
            _shiftDictionary = new Dictionary<IShiftCategory, int>();
    		_fairnessValueResult = new FairnessValueResult();
        }

        public ShiftCategoryFairnessHolder(IDictionary<IShiftCategory, int> shiftDictionary, IFairnessValueResult fairnessValueResult)
        {
        	_shiftDictionary = shiftDictionary;
        	_fairnessValueResult = fairnessValueResult;
        }

    	public IFairnessValueResult FairnessValueResult
    	{
    		get { return _fairnessValueResult; }
    	}

    	public IDictionary<IShiftCategory, int> ShiftCategoryFairnessDictionary
        {
            get
            {
                if (_shiftDictionary == null)
                    _shiftDictionary = new Dictionary<IShiftCategory, int>();
                return new ReadOnlyDictionary<IShiftCategory, int>(_shiftDictionary);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IShiftCategoryFairnessHolder Add(IShiftCategoryFairnessHolder holder)
        {
            Dictionary<IShiftCategory, int> result = new Dictionary<IShiftCategory, int>();

            foreach (KeyValuePair<IShiftCategory, int> keyValuePair in ShiftCategoryFairnessDictionary)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }

            foreach (KeyValuePair<IShiftCategory, int> keyValuePair in holder.ShiftCategoryFairnessDictionary)
            {
                if (!result.ContainsKey(keyValuePair.Key))
                    result.Add(keyValuePair.Key, 0);
                result[keyValuePair.Key] += keyValuePair.Value;
            }

        	IFairnessValueResult fairnessValueResult = new FairnessValueResult();
        	fairnessValueResult.FairnessPoints = _fairnessValueResult.FairnessPoints +
        	                                     holder.FairnessValueResult.FairnessPoints;
        	fairnessValueResult.TotalNumberOfShifts = _fairnessValueResult.TotalNumberOfShifts +
        	                                          holder.FairnessValueResult.TotalNumberOfShifts;

            return new ShiftCategoryFairnessHolder(result, fairnessValueResult);
        }

        #region Equals stuff

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Equals(IShiftCategoryFairnessHolder other)
        {
            if (ShiftCategoryFairnessDictionary.Keys.Count != other.ShiftCategoryFairnessDictionary.Keys.Count)
                return false;

            IList<IShiftCategory> thisList =
            new List<IShiftCategory>(ShiftCategoryFairnessDictionary.Keys);

            IList<IShiftCategory> otherList =
            new List<IShiftCategory>(other.ShiftCategoryFairnessDictionary.Keys);

            for (int i = 0; i < thisList.Count; i++)
            {
                if (!otherList[i].Equals(thisList[i]))
                    return false;
                if (other.ShiftCategoryFairnessDictionary[otherList[i]] != ShiftCategoryFairnessDictionary[thisList[i]])
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
			var ent = obj as IShiftCategoryFairnessHolder;
			if (ent == null)
				return false;
			return Equals(ent);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            int result = 0;
            foreach (KeyValuePair<IShiftCategory, int> keyValuePair in ShiftCategoryFairnessDictionary)
            {
                result = result ^ keyValuePair.GetHashCode();
            }
            return result;
        }

        #endregion
    }
}