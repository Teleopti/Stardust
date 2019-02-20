using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftCategoryDistributionModelTest
	{
		private MockRepository _mocks;
		private ShiftCategoryDistributionModel _target;
		private ICachedNumberOfEachCategoryPerPerson _cachedNumberOfEachCategoryPerPerson;
		private ICachedNumberOfEachCategoryPerDate _cachedNumberOfEachCategoryPerDate;
		private ICachedShiftCategoryDistribution _cachedShiftCategoryDistribution;
		private bool _chartUpdateNeeded;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_cachedNumberOfEachCategoryPerPerson = _mocks.StrictMock<ICachedNumberOfEachCategoryPerPerson>();
			_cachedNumberOfEachCategoryPerDate = _mocks.StrictMock<ICachedNumberOfEachCategoryPerDate>();
			_cachedShiftCategoryDistribution = _mocks.StrictMock<ICachedShiftCategoryDistribution>();
			_target = new ShiftCategoryDistributionModel(_cachedShiftCategoryDistribution, _cachedNumberOfEachCategoryPerDate, _cachedNumberOfEachCategoryPerPerson,
														 new DateOnlyPeriod(2013, 09, 16, 2013, 09, 17), new CommonNameDescriptionSetting());
			_target.ChartUpdateNeeded += targetChartUpdateNeeded;
		}

		void targetChartUpdateNeeded(object sender, System.EventArgs e)
		{
			_chartUpdateNeeded = true;
		}

		[Test]
		public void ShouldGetSortedPersonsSorted()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var person2 = PersonFactory.CreatePerson("Adam");
			var filteredPersons = new List<IPerson> { person1, person2 };
			
			using (_mocks.Record())
			{
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons)).Repeat.AtLeastOnce();
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons)).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				Assert.AreEqual(person2, _target.GetSortedPersons(true)[0]);
				Assert.AreEqual(person1, _target.GetSortedPersons(false)[0]);
			}
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
		public void ShouldGetShiftCategoriesSortedByMin()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var filteredPersons = new List<IPerson> { person1 };
			var minMaxDic = new Dictionary<IShiftCategory, MinMax<int>>();

			var cat1 = new ShiftCategory("B");
			var cat2 = new ShiftCategory("A");
			var minMax1 = new MinMax<int>(5, 10);
			var minMax2 = new MinMax<int>(1, 10);
			minMaxDic.Add(cat1, minMax1);
			minMaxDic.Add(cat2, minMax2);


			using (_mocks.Record())
			{
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons));
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons));
				Expect.Call(_cachedShiftCategoryDistribution.AllShiftCategories).Return(new List<IShiftCategory> { cat1, cat2 }).Repeat.AtLeastOnce();
				Expect.Call(_cachedShiftCategoryDistribution.GetMinMaxDictionary(filteredPersons)).Return(minMaxDic).Repeat.AtLeastOnce();

			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				var result = _target.GetShiftCategoriesSortedByMinMax(true, true);
				Assert.AreEqual(cat2, result[0]);
				Assert.AreEqual(cat1, result[1]);
				result = _target.GetShiftCategoriesSortedByMinMax(false, true);
				Assert.AreEqual(cat1, result[0]);
				Assert.AreEqual(cat2, result[1]);
			}	
		}

		[Test]
		public void ShouldGetShiftCategoriesSortedByMax()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var filteredPersons = new List<IPerson> { person1 };
			var minMaxDic = new Dictionary<IShiftCategory, MinMax<int>>();

			var cat1 = new ShiftCategory("B");
			var cat2 = new ShiftCategory("A");
			var minMax1 = new MinMax<int>(5, 10);
			var minMax2 = new MinMax<int>(1, 8);
			minMaxDic.Add(cat1, minMax1);
			minMaxDic.Add(cat2, minMax2);


			using (_mocks.Record())
			{
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons));
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons));
				Expect.Call(_cachedShiftCategoryDistribution.AllShiftCategories).Return(new List<IShiftCategory> { cat1, cat2 }).Repeat.AtLeastOnce();
				Expect.Call(_cachedShiftCategoryDistribution.GetMinMaxDictionary(filteredPersons)).Return(minMaxDic).Repeat.AtLeastOnce();

			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				var result = _target.GetShiftCategoriesSortedByMinMax(true, false);
				Assert.AreEqual(cat2, result[0]);
				Assert.AreEqual(cat1, result[1]);
				result = _target.GetShiftCategoriesSortedByMinMax(false, false);
				Assert.AreEqual(cat1, result[0]);
				Assert.AreEqual(cat2, result[1]);
			}
		}

		[Test]
		public void ShouldGetShiftCategoriesSortedByAverage()
		{
			var person1 = PersonFactory.CreatePerson("Bertil");
			var filteredPersons = new List<IPerson> { person1 };
			var dic = new Dictionary<IShiftCategory, int>();

			var cat1 = new ShiftCategory("B");
			var cat2 = new ShiftCategory("A");

			dic.Add(cat1, 6);
			dic.Add(cat2, 3);


			using (_mocks.Record())
			{
				Expect.Call(() => _cachedNumberOfEachCategoryPerDate.SetFilteredPersons(filteredPersons));
				Expect.Call(() => _cachedShiftCategoryDistribution.SetFilteredPersons(filteredPersons));
				Expect.Call(_cachedShiftCategoryDistribution.AllShiftCategories).Return(new List<IShiftCategory> { cat1, cat2 }).Repeat.AtLeastOnce();
				Expect.Call(_cachedNumberOfEachCategoryPerPerson.GetValue(person1)).Return(dic).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_target.SetFilteredPersons(filteredPersons);
				var result = _target.GetShiftCategoriesSortedByAverage(true);
				Assert.AreEqual(cat2, result[0]);
				Assert.AreEqual(cat1, result[1]);
				result = _target.GetShiftCategoriesSortedByAverage(false);
				Assert.AreEqual(cat1, result[0]);
				Assert.AreEqual(cat2, result[1]);
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

		[Test]
		public void ShouldRaiseEventOnChartUpdateNeeded()
		{
			_target.OnChartUpdateNeeded();
			Assert.IsTrue(_chartUpdateNeeded);
		}

		[Test]
		public void ShouldGetSetIfShouldUpdateViews()
		{
			Assert.IsFalse(_target.ShouldUpdateViews);
			_target.ShouldUpdateViews = true;
			Assert.IsTrue(_target.ShouldUpdateViews);
		}
	}
}