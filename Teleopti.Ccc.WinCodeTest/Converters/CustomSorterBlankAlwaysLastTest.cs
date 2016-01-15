using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
	[TestFixture, SetCulture("sv-SE")]
	public class CustomSorterBlankAlwaysLastTest
	{
		[Test]
		public void Sort_WhenAscendingAndNoBlanks_ShouldSortAscending()
		{
			var sortlist = new List<SomethingThatIsSortable>()
				               {
					               new SomethingThatIsSortable(){Number = 1},
					               new SomethingThatIsSortable(){Number = 2},
					               new SomethingThatIsSortable(){Number = 3},
					               new SomethingThatIsSortable(){Number = 4},
					               new SomethingThatIsSortable(){Number = 5},
				               };

			var target = new CustomSorterBlankAlwaysLast() { PropertyPath = "Number", SortDirection = ListSortDirection.Ascending };
			var view = (ListCollectionView)CollectionViewSource.GetDefaultView(sortlist);
			view.CustomSort = target;

			var expectedSequence = new List<int>() { 1, 2, 3, 4, 5 };
			var actualSequence = getSequence(view);
			CollectionAssert.AreEqual(actualSequence,expectedSequence);
		}

		[Test]
		public void Sort_WhenDescendingAndNoBlanks_ShouldSortDescending()
		{
			var sortlist = new List<SomethingThatIsSortable>()
				               {
					               new SomethingThatIsSortable{Number = 1},
					               new SomethingThatIsSortable{Number = 3},
					               new SomethingThatIsSortable{Number = 2},
					               new SomethingThatIsSortable{Number = 4},
					               new SomethingThatIsSortable{Number = 5},
				               };

			var target = new CustomSorterBlankAlwaysLast() { PropertyPath = "Number", SortDirection = ListSortDirection.Descending };
			var view = (ListCollectionView)CollectionViewSource.GetDefaultView(sortlist);
			view.CustomSort = target;

			var expectedSequence = new List<int>() { 5, 4, 3, 2, 1 };
			var actualSequence = getSequence(view);
			CollectionAssert.AreEqual(actualSequence, expectedSequence);
		}

		[Test]
		public void Sort_WhenAscending_ShouldSortAscendingAndPutBlanksLast()
		{
			var sortlist = new List<SomethingThatIsSortable>()
				               {
					               new SomethingThatIsSortable {Number = 1,Name = "A"},
					               new SomethingThatIsSortable {Number =  2, Name = ""},
					               new SomethingThatIsSortable {Number = 3, Name = "B"},
					               new SomethingThatIsSortable {Number = 4, Name = "C"},
					               new SomethingThatIsSortable {Number = 5, Name = "D"},
				               };

			var target = new CustomSorterBlankAlwaysLast() { PropertyPath = "Name", SortDirection = ListSortDirection.Ascending };
			var view = (ListCollectionView)CollectionViewSource.GetDefaultView(sortlist);
			view.CustomSort = target;
			var expectedSequence = new List<int>() { 1, 3, 4, 5, 2 };

			var actualSequence = getSequence(view);
			CollectionAssert.AreEqual(actualSequence, expectedSequence);
		}

		[Test]
		public void Sort_WhenDescending_ShouldSortDescendingAndPutBlanksLast()
		{
			var sortlist = new List<SomethingThatIsSortable>
			{
					               new SomethingThatIsSortable {Number = 1,Name = "D"},
					               new SomethingThatIsSortable {Number=2, Name = ""},
					               new SomethingThatIsSortable {Number = 3, Name = "C"},
					               new SomethingThatIsSortable {Number = 4, Name = "B"},
					               new SomethingThatIsSortable {Number = 5, Name = "A"},
				               };

			var target = new CustomSorterBlankAlwaysLast { PropertyPath = "Name", SortDirection = ListSortDirection.Descending };
			var view = (ListCollectionView)CollectionViewSource.GetDefaultView(sortlist);
			view.CustomSort = target;

			var expectedSequence = new List<int> { 1, 3, 4, 5, 2 };

			var actualSequence = getSequence(view);
			CollectionAssert.AreEqual(actualSequence, expectedSequence);
		}

		[Test]
		public void Sort_WhenAscending_ShouldSortDescendingAndPutNullLast()
		{
			var sortlist = new List<SomethingThatIsSortable>
			{
					               new SomethingThatIsSortable {Number = 1, AnObject = new SomethingThatCanBeNull {Sortvalue = "D"}},
					               new SomethingThatIsSortable {Number=2, AnObject = null },
					               new SomethingThatIsSortable {Number = 3, AnObject = new SomethingThatCanBeNull {Sortvalue = "C"}, },
					               new SomethingThatIsSortable {Number = 4, AnObject = new SomethingThatCanBeNull {Sortvalue = "B"}},
					               new SomethingThatIsSortable {Number = 5, AnObject = new SomethingThatCanBeNull {Sortvalue = "A"}},
				               };

			var target = new CustomSorterBlankAlwaysLast { PropertyPath = "AnObject", SortDirection = ListSortDirection.Descending };
			var view = (ListCollectionView)CollectionViewSource.GetDefaultView(sortlist);
			view.CustomSort = target;

			var expectedSequence = new List<int> { 1, 3, 4, 5, 2 };

			var actualSequence = getSequence(view);
			CollectionAssert.AreEqual(actualSequence, expectedSequence);
		}

		[Test]
		public void Sort_WhenAscending_ShouldSortAscendingAndPutNullLast()
		{
			var sortlist = new List<SomethingThatIsSortable>
			{
								    new SomethingThatIsSortable {Number = 1, AnObject = new SomethingThatCanBeNull {Sortvalue = "A"}},
					               new SomethingThatIsSortable {Number=2, AnObject = null },
					               new SomethingThatIsSortable {Number = 3, AnObject = new SomethingThatCanBeNull {Sortvalue = "B"}, },
					               new SomethingThatIsSortable {Number = 4, AnObject = new SomethingThatCanBeNull {Sortvalue = "C"}},
					               new SomethingThatIsSortable {Number = 5, AnObject = new SomethingThatCanBeNull {Sortvalue = "D"}},
				               };

			var target = new CustomSorterBlankAlwaysLast { PropertyPath = "AnObject", SortDirection = ListSortDirection.Ascending };
			var view = (ListCollectionView)CollectionViewSource.GetDefaultView(sortlist);
			view.CustomSort = target;
			var expectedSequence = new List<int> { 1, 3, 4, 5, 2 };

			var actualSequence = getSequence(view);
			CollectionAssert.AreEqual(actualSequence, expectedSequence);
		}

		[Test]
		public void SortAscending_WhenSortingDateTimes_ShouldPutEmptyDateTimesLast_PBI34510()
		{
			var start = new DateTime(2001, 1, 1, 1, 0, 0);

			var sortlist = new List<SomethingThatIsSortable>
			{
							   new SomethingThatIsSortable {Number = 1, Date = start},
							   new SomethingThatIsSortable {Number = 2, Date = start.AddDays(1)},
							   new SomethingThatIsSortable {Number = 3, Date = start.AddDays(-1)},
							   new SomethingThatIsSortable {Number = 4 }
						   };
			var target = new CustomSorterBlankAlwaysLast { PropertyPath = "Date", SortDirection = ListSortDirection.Ascending };
			var view = (ListCollectionView)CollectionViewSource.GetDefaultView(sortlist);
			view.CustomSort = target;
			var expectedSequence = new List<int> { 3, 1, 2, 4 };

			var actualSequence = getSequence(view);
			CollectionAssert.AreEqual(actualSequence, expectedSequence);
		}

		[Test]
		public void Sort_WhenSortingDateTimesDescending_ShouldPutEmptyDateTimesLast_PBI34510()
		{
			var start = new DateTime(2001, 1, 1, 1, 0, 0);

			var sortlist = new List<SomethingThatIsSortable>
			{
							   new SomethingThatIsSortable {Number = 1, Date = start},
							   new SomethingThatIsSortable {Number = 2, Date = start.AddDays(1)},
							   new SomethingThatIsSortable {Number = 3, Date = start.AddDays(-1)},
							   new SomethingThatIsSortable {Number = 4 }
						   };
			var target = new CustomSorterBlankAlwaysLast { PropertyPath = "Date", SortDirection = ListSortDirection.Descending };
			var view = (ListCollectionView)CollectionViewSource.GetDefaultView(sortlist);
			view.CustomSort = target;
			var expectedSequence = new List<int>() { 2, 1, 3, 4 };

			var actualSequence = getSequence(view);
			CollectionAssert.AreEqual(actualSequence, expectedSequence);
		}
		
		#region helpers
		
		private static IList<int> getSequence(ListCollectionView collectionView)
		{
			var ret = new List<int>();
			collectionView.MoveCurrentToFirst();

			while (!collectionView.IsCurrentAfterLast)
			{
				ret.Add(((SomethingThatIsSortable)collectionView.CurrentItem).Number);
				collectionView.MoveCurrentToNext();
			}

			return ret;
		}
		
		public class SomethingThatIsSortable
		{
			public int Number { get; set; }

			public string Name { get; set; }

			public SomethingThatCanBeNull AnObject { get; set; }
			
			public DateTime Date { get; set; }
		}

		public class SomethingThatCanBeNull
		{
			public string Sortvalue { get; set; }

			public override string ToString()
			{
				return Sortvalue;
			}
		}
		#endregion
	}
}