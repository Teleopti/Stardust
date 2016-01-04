﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    public class SkillStaffPeriodDictionary : ISkillStaffPeriodDictionary
    {
        private Lazy<IList<DateTimePeriod>> _openHoursCollection;
	    private Lazy<ILookup<HourSlot, ISkillStaffPeriod>> _lookup;
        private readonly IDictionary<DateTimePeriod, ISkillStaffPeriod> _wrappedDictionary = new Dictionary<DateTimePeriod, ISkillStaffPeriod>();
        private readonly IAggregateSkill _skill;

	    private SkillStaffPeriodDictionary()
	    {
		    _openHoursCollection = new Lazy<IList<DateTimePeriod>>(createOpenHourCollection);
			_lookup = new Lazy<ILookup<HourSlot, ISkillStaffPeriod>>(createLookup);
	    }

        public SkillStaffPeriodDictionary(IAggregateSkill skill)
            : this()
        {
            _skill = skill;
        }

        #region Implementation of ISkillStaffPeriodDictionary

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        public bool ContainsKey(DateTimePeriod key)
        {
            return _wrappedDictionary.ContainsKey(key);
        }

        /// <summary>
        ///                     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///                     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<KeyValuePair<DateTimePeriod, ISkillStaffPeriod>> IEnumerable<KeyValuePair<DateTimePeriod, ISkillStaffPeriod>>.GetEnumerator()
        {
            return _wrappedDictionary.GetEnumerator();
        }

 
        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        public bool Remove(DateTimePeriod key)
        {
			var remove = _wrappedDictionary.Remove(key);
	        if (remove)
	        {
		        _openHoursCollection = new Lazy<IList<DateTimePeriod>>(createOpenHourCollection);
		        _lookup = new Lazy<ILookup<HourSlot, ISkillStaffPeriod>>(createLookup);
	        }
	        return remove;
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        public bool TryGetValue(DateTimePeriod key, out ISkillStaffPeriod value)
        {
            return _wrappedDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Teleopti.Interfaces.Domain.ISkillStaffPeriod"/> with the specified key.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        public ISkillStaffPeriod this[DateTimePeriod key]
        {
            get { return _wrappedDictionary[key]; }
            set { throw new NotImplementedException(); }
        }


        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-30
        /// </remarks>
        public ICollection<DateTimePeriod> Keys
        {
            get
            {
                return _wrappedDictionary.Keys;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-30
        /// </remarks>
        public ICollection<ISkillStaffPeriod> Values
        {
            get
            {
                return _wrappedDictionary.Values;
            }
        }

	    private ILookup<HourSlot, ISkillStaffPeriod> createLookup()
	    {
			return _wrappedDictionary.Values.ToLookup(v => new HourSlot(v.Period.StartDateTime));
	    }

	    public ILookup<HourSlot, ISkillStaffPeriod> ForLookup()
	    {
		    return _lookup.Value;
	    }

        /// <summary>
        ///                     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <param name="item">
        ///                     The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///                 </param>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        ///                 </exception>
        public bool Remove(KeyValuePair<DateTimePeriod, ISkillStaffPeriod> item)
        {
            return Remove(item.Key);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        public int Count
        {
            get { return _wrappedDictionary.Count; }
        }

        /// <summary>
        ///                     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return _wrappedDictionary.IsReadOnly; }
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        public void Add(DateTimePeriod key, ISkillStaffPeriod value)
        {
            Add(new KeyValuePair<DateTimePeriod, ISkillStaffPeriod>(key, value));
        }

        /// <summary>
        ///                     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">
        ///                     The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///                 </param>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        ///                 </exception>
        public void Add(KeyValuePair<DateTimePeriod, ISkillStaffPeriod> item)
        {
            if(item.Key != item.Value.Period)
                throw new InvalidConstraintException("item.key must equal item.value.period");

            _wrappedDictionary.Add(item);
			_openHoursCollection = new Lazy<IList<DateTimePeriod>>(createOpenHourCollection);
			_lookup = new Lazy<ILookup<HourSlot, ISkillStaffPeriod>>(createLookup);
        }

        public void Add(ISkillStaffPeriod skillStaffPeriod)
        {
            Add(skillStaffPeriod.Period, skillStaffPeriod);
        }

        /// <summary>
        /// Gets the skill of the skillstaff periods in this collection.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-12
        /// </remarks>
        public IAggregateSkill Skill
        {
            get { return _skill; }
        }

        public bool TryGetResolutionAdjustedValue(DateTimePeriod key, out ISkillStaffPeriod value)
        {
            int defaultRes = ((ISkill) _skill).DefaultResolution;
            DateTimePeriod adjustedKey = key;
            if(key.ElapsedTime().TotalMinutes != defaultRes)
            {
                DateTime adjustedStart =
                    key.StartDateTime.Date.Add(TimeHelper.FitToDefaultResolution(key.StartDateTime.TimeOfDay, defaultRes));
                adjustedKey = new DateTimePeriod(adjustedStart, adjustedStart.AddMinutes(defaultRes));
            }

            return _wrappedDictionary.TryGetValue(adjustedKey, out value);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        public void Clear()
        {
            _wrappedDictionary.Clear();
			_openHoursCollection = new Lazy<IList<DateTimePeriod>>(createOpenHourCollection);
			_lookup = new Lazy<ILookup<HourSlot, ISkillStaffPeriod>>(createLookup);
        }

        /// <summary>
        ///                     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <param name="item">
        ///                     The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///                 </param>
        public bool Contains(KeyValuePair<DateTimePeriod, ISkillStaffPeriod> item)
        {
            return ContainsKey(item.Key);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(KeyValuePair<DateTimePeriod, ISkillStaffPeriod>[] array, int arrayIndex)
        {
            _wrappedDictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the skill open hours collection.
        /// </summary>
        /// <value>The skill open hours collection.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        public ReadOnlyCollection<DateTimePeriod> SkillOpenHoursCollection
        {
	        get { return new ReadOnlyCollection<DateTimePeriod>(_openHoursCollection.Value); }
        }

        #endregion

		private IList<DateTimePeriod> createOpenHourCollection()
        {
            var openHoursCollection = new List<DateTimePeriod>();

            if (_wrappedDictionary.Count == 0)
                return openHoursCollection;

            IList<DateTimePeriod> sortedList = _wrappedDictionary.Keys.OrderBy(s => s.StartDateTime).ToList();
            DateTimePeriod current = sortedList[0];
            DateTime currentStart = current.StartDateTime;
            DateTime currentEnd = current.EndDateTime;
            for (int index = 1; index < sortedList.Count; index++)
            {
                current = sortedList[index];
                if (current.StartDateTime == currentEnd)
                {
                    currentEnd = current.EndDateTime;
                }
                else
                {
                    openHoursCollection.Add(new DateTimePeriod(currentStart, currentEnd));
                    currentStart = current.StartDateTime;
                    currentEnd = current.EndDateTime;
                }
            }
            openHoursCollection.Add(new DateTimePeriod(currentStart, currentEnd));

			return openHoursCollection;
        }

        #region Implementation of IEnumerable

        /// <summary>
        ///                     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///                     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            return _wrappedDictionary.GetEnumerator();
        }

        #endregion
    }
}
