﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftCategoryDistributionModelTest
	{
		private MockRepository _mocks;
		private IShiftCategoryDistributionModel _target;
		private ICachedNumberOfEachCategoryPerPerson _cachedNumberOfEachCategoryPerPerson;
		private ICachedNumberOfEachCategoryPerDate _cachedNumberOfEachCategoryPerDate;
		private ICachedShiftCategoryDistribution _cachedShiftCategoryDistribution;
		private ISchedulerStateHolder _schedulerStateHolder;
	    private IPopulationStatisticsCalculator _populationStatisticsCalculator;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_cachedNumberOfEachCategoryPerPerson = _mocks.StrictMock<ICachedNumberOfEachCategoryPerPerson>();
			_cachedNumberOfEachCategoryPerDate = _mocks.StrictMock<ICachedNumberOfEachCategoryPerDate>();
			_cachedShiftCategoryDistribution = _mocks.StrictMock<ICachedShiftCategoryDistribution>();
			_schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            _populationStatisticsCalculator = new PopulationStatisticsCalculator(true );
			_target = new ShiftCategoryDistributionModel(_cachedShiftCategoryDistribution, _cachedNumberOfEachCategoryPerDate, _cachedNumberOfEachCategoryPerPerson,
			                                             new DateOnlyPeriod(2013, 09, 16, 2013, 09, 17), _schedulerStateHolder,_populationStatisticsCalculator );
		}

		[Test]
		public void ShouldGetSortedPersonsSorted()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var person2 = PersonFactory.CreatePerson("Adam");
			var filteredPersons = new List<IPerson> {person1, person2};
			_target.SetFilteredPersons(filteredPersons);
			Assert.AreEqual(person2, _target.GetSortedPersons(true)[0]);
			Assert.AreEqual(person1, _target.GetSortedPersons(false)[0]);
		}

		[Test]
		public void ShouldGetDatesSorted()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var filteredPersons = new List<IPerson> { person1 };
			_target.SetFilteredPersons(filteredPersons);
			Assert.AreEqual(new DateOnly(2013, 9, 16), _target.GetSortedDates(true)[0]);
			Assert.AreEqual(new DateOnly(2013, 9, 17), _target.GetSortedDates(false)[0]);
		}

		[Test]
		public void ShouldGetSortedShiftCategories()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var filteredPersons = new List<IPerson> {person1};

			var cat1 = new ShiftCategory("B");
			var cat2 = new ShiftCategory("A");
			IDictionary<IShiftCategory, int> value = new Dictionary<IShiftCategory, int>();
			value.Add(cat1, 0);
			value.Add(cat2, 0);

			using (_mocks.Record())
			{
				//Expect.Call(_cachedNumberOfEachCategoryPerPerson.GetValue(person1)).Return(value);
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons));
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons));
				Expect.Call(_cachedShiftCategoryDistribution.AllShiftCategories).Return(new List<IShiftCategory> {cat1, cat2});
			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				var result = _target.GetSortedShiftCategories();
				Assert.AreEqual(cat2, result[0]);
				Assert.AreEqual(cat1, result[1]);
			}
		}

		[Test]
		public void ShouldGetShiftCategoryCountForCategoryAndPerson()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var filteredPersons = new List<IPerson> { person1 };

			var cat1 = new ShiftCategory("B");
			var cat2 = new ShiftCategory("A");
			IDictionary<IShiftCategory, int> value = new Dictionary<IShiftCategory, int>();
			value.Add(cat1, 0);
			value.Add(cat2, 33);

			using (_mocks.Record())
			{
				Expect.Call(_cachedNumberOfEachCategoryPerPerson.GetValue(person1)).Return(value);
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons));
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons));
			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				Assert.AreEqual(33, _target.ShiftCategoryCount(person1, cat2));
			}
		}

		[Test]
		public void ShouldGetShiftCategoryCountForCategoryAndDate()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var filteredPersons = new List<IPerson> { person1 };

			var cat1 = new ShiftCategory("B");
			var cat2 = new ShiftCategory("A");
			IDictionary<IShiftCategory, int> value = new Dictionary<IShiftCategory, int>();
			value.Add(cat1, 0);
			value.Add(cat2, 33);

			using (_mocks.Record())
			{
				Expect.Call(_cachedNumberOfEachCategoryPerDate.GetValue(new DateOnly(2013, 9, 16))).Return(value);
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons));
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons));
			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				Assert.AreEqual(33, _target.ShiftCategoryCount(new DateOnly(2013, 9, 16), cat2));
			}
		}

		[Test]
		public void ShouldGetAgentsSortedByNumberOfShiftCategories()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var person2 = PersonFactory.CreatePerson("Adam");
			var filteredPersons = new List<IPerson> { person1, person2 };
			

			var cat1 = new ShiftCategory("B");
			IDictionary<IShiftCategory, int> value1 = new Dictionary<IShiftCategory, int>();
			value1.Add(cat1, 0);
			IDictionary<IShiftCategory, int> value2 = new Dictionary<IShiftCategory, int>();
			value2.Add(cat1, 33);

			using (_mocks.Record())
			{
				Expect.Call(_cachedNumberOfEachCategoryPerPerson.GetValue(person1)).Return(value1);
				Expect.Call(_cachedNumberOfEachCategoryPerPerson.GetValue(person2)).Return(value2);
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons));
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons));
			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				Assert.AreEqual(person2, _target.GetAgentsSortedByNumberOfShiftCategories(cat1, false)[0]);
			}
		}

		[Test]
		public void ShouldGetDatesSortedByNumberOfShiftCategories()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var person2 = PersonFactory.CreatePerson("Adam");
			var filteredPersons = new List<IPerson> { person1, person2 };

			var cat1 = new ShiftCategory("B");
			IDictionary<IShiftCategory, int> value1 = new Dictionary<IShiftCategory, int>();
			value1.Add(cat1, 0);
			IDictionary<IShiftCategory, int> value2 = new Dictionary<IShiftCategory, int>();
			value2.Add(cat1, 33);

			using (_mocks.Record())
			{
				Expect.Call(_cachedNumberOfEachCategoryPerDate.GetValue(new DateOnly(2013, 9, 16))).Return(value1);
				Expect.Call(_cachedNumberOfEachCategoryPerDate.GetValue(new DateOnly(2013, 9, 17))).Return(value2);
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons));
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons));
			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				Assert.AreEqual(new DateOnly(2013, 9, 17), _target.GetDatesSortedByNumberOfShiftCategories(cat1, false)[0]);
			}
		}
	}
}