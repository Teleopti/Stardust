using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    public class SkillStaffPeriodDictionary : ISkillStaffPeriodDictionary
	{
        private Lazy<IList<DateTimePeriod>> _openHoursCollection;
        private readonly IDictionary<DateTimePeriod, ISkillStaffPeriod> _wrappedDictionary = new Dictionary<DateTimePeriod, ISkillStaffPeriod>();
		private readonly IDictionary<DateTime, HashSet<DateTimePeriod>> _index = new Dictionary<DateTime, HashSet<DateTimePeriod>>();
        private readonly IAggregateSkill _skill;

	    private SkillStaffPeriodDictionary()
	    {
		    _openHoursCollection = new Lazy<IList<DateTimePeriod>>(createOpenHourCollection);
	    }

        public SkillStaffPeriodDictionary(IAggregateSkill skill) : this()
        {
            _skill = skill;
        }

		public SkillStaffPeriodDictionary(IAggregateSkill skill, IDictionary<DateTimePeriod, ISkillStaffPeriod> wrappedDictionary) : this(skill)
		{
			_wrappedDictionary = wrappedDictionary;
			_index = wrappedDictionary.Keys.SelectMany(k => k.Keys().Select(p => (p, k))).GroupBy(k => k.Item1)
				.ToDictionary(k => k.Key, v => v.Select(i => i.Item2).ToHashSet());
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

	    public IDictionary<DateTimePeriod, ISkillStaffPeriod> InnerDictionary()
	    {
		    return _wrappedDictionary;
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
				key.Keys().ForEach(k =>
				{
					if (_index.TryGetValue(k, out var periods))
					{
						periods.Remove(key);
						if (periods.Count == 0)
						{
							_index.Remove(k);
						}
					}
				});
				_openHoursCollection = new Lazy<IList<DateTimePeriod>>(createOpenHourCollection);
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
        /// Gets or sets the <see cref="ISkillStaffPeriod"/> with the specified key.
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
        public ICollection<DateTimePeriod> Keys => _wrappedDictionary.Keys;

		/// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-30
        /// </remarks>
        public ICollection<ISkillStaffPeriod> Values => _wrappedDictionary.Values;

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
        public int Count => _wrappedDictionary.Count;

		/// <summary>
        ///                     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => _wrappedDictionary.IsReadOnly;

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
			item.Key.Keys().ForEach(k =>
			{
				if (_index.TryGetValue(k, out var periods))
				{
					periods.Add(item.Key);
				}
				else
				{
					_index.Add(k, new HashSet<DateTimePeriod> { item.Key });
				}
			});
			_openHoursCollection = new Lazy<IList<DateTimePeriod>>(createOpenHourCollection);
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
        public IAggregateSkill Skill => _skill;

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
			_index.Clear();
			_openHoursCollection = new Lazy<IList<DateTimePeriod>>(createOpenHourCollection);
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
        public ReadOnlyCollection<DateTimePeriod> SkillOpenHoursCollection => new ReadOnlyCollection<DateTimePeriod>(_openHoursCollection.Value);

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

		public IEnumerable<KeyValuePair<DateTimePeriod, IResourceCalculationPeriod>> Items()
		{
			return _wrappedDictionary.Select(w => new KeyValuePair<DateTimePeriod, IResourceCalculationPeriod>(w.Key, w.Value));
		}

		public IEnumerable<IResourceCalculationPeriod> FindUsingIndex(DateTimePeriod period)
		{
			var keys = period.Keys();
			foreach (var key in keys)
			{
				if (!_index.TryGetValue(key, out var innerKeys)) continue;
				foreach (var innerKey in innerKeys)
				{
					if (innerKey.Intersect(period) && _wrappedDictionary.TryGetValue(innerKey, out var skillStaffPeriod))
					{
						yield return skillStaffPeriod;
					}
				}
			}
		}

		public bool TryGetValue(DateTimePeriod dateTimePeriod, out IResourceCalculationPeriod resourceCalculationPeriod)
		{
			if (_wrappedDictionary.TryGetValue(dateTimePeriod, out var found))
			{
				resourceCalculationPeriod = found;
				return true;

			}
			resourceCalculationPeriod = null;
			return false;
		}

		IEnumerable<IResourceCalculationPeriod> IResourceCalculationPeriodDictionary.OnlyValues()
		{
			return _wrappedDictionary.Values;
		}
	}

	public static class DateTimePeriodKeyExtensions
	{
		public static IEnumerable<DateTime> Keys(this DateTimePeriod period)
		{
			var hour = period.StartDateTime.Truncate(TimeSpan.FromHours(1));
			while (hour<period.EndDateTime)
			{
				yield return hour;
				hour = hour.AddHours(1);
			}
		}
	}

	public static class SkillDictionaryExtensions
	{
		public static bool TryGetResolutionAdjustedValue(this IResourceCalculationPeriodDictionary source, ISkill skill, DateTimePeriod key, out IResourceCalculationPeriod value)
		{
			int defaultRes = skill.DefaultResolution;
			DateTimePeriod adjustedKey = key;
			if (key.ElapsedTime().TotalMinutes != defaultRes)
			{
				DateTime adjustedStart =
					key.StartDateTime.Date.Add(TimeHelper.FitToDefaultResolution(key.StartDateTime.TimeOfDay, defaultRes));
				adjustedKey = new DateTimePeriod(adjustedStart, adjustedStart.AddMinutes(defaultRes));
			}

			return source.TryGetValue(adjustedKey, out value);
		}
	}
}
