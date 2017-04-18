using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
	public class ShiftCategoryLimitationViewPresenter : IDisposable
	{
		private IList<ISchedulePeriod> _schedulePeriods = new List<ISchedulePeriod>();
		private IShiftCategoryLimitationView _view;

		private IDictionary<IShiftCategory, ShiftCategoryLimitationCombination> _combinedLimitations =
			new Dictionary<IShiftCategory, ShiftCategoryLimitationCombination>();

		private List<IShiftCategory> _shiftCategories = new List<IShiftCategory>();


		public ShiftCategoryLimitationViewPresenter(IShiftCategoryLimitationView shiftCategoryLimitationView)
		{
			_view = shiftCategoryLimitationView;
		}

		public void Initialize()
		{
			SetShiftCategories(_view.ShiftCategories);
			_view.SetupGrid();
		}

		public void AddShiftCategory(IShiftCategory shiftCategory)
		{
			if (!_shiftCategories.Contains(shiftCategory))
				_shiftCategories.Add(shiftCategory);
		}

		public IList<ISchedulePeriod> SchedulePeriods
		{
			get
			{ return _schedulePeriods; }
		}

		public IDictionary<IShiftCategory, ShiftCategoryLimitationCombination> CombinedLimitations
		{
			get { return _combinedLimitations; }
		}

		public IList<IShiftCategory> ShiftCategories
		{
			get
			{
				return (from cat in _shiftCategories orderby cat.Description.Name ascending select cat).ToList();
			}
		}

		public void SetShiftCategories(IList<IShiftCategory> shiftCategories)
		{
			CombinedLimitations.Clear();
			foreach (var category in shiftCategories)
			{
				AddShiftCategory(category);
				var comb = new ShiftCategoryLimitationCombination(category);
				CombinedLimitations.Add(category, comb);
			}
		}

		public void SetSchedulePeriodList(IList<ISchedulePeriod> schedulePeriods)
		{
			if (schedulePeriods == null)
				_schedulePeriods.Clear();

			_schedulePeriods = schedulePeriods;
			CreateCombined();
		}

		public void CreateCombined()
		{
			foreach (var category in CombinedLimitations.Keys)
			{
				CombinedLimitations[category].Clear();
			}

			foreach (var schedulePeriod in _schedulePeriods)
			{

				foreach (var shiftCategory in CombinedLimitations.Keys)
				{
					bool found = false;
					foreach (var limitation in schedulePeriod.ShiftCategoryLimitationCollection())
					{
						if (limitation.ShiftCategory.Id == shiftCategory.Id)
						{
							found = true;
							CombinedLimitations[shiftCategory].CombineLimitations(limitation);
						}
					}
					if (!found)
						CombinedLimitations[shiftCategory].CombineWithNoLimitation(shiftCategory);
				}
			}
		}

		public static string OnQueryHeaderInfo(int rowIndex, int colIndex)
		{
			if (rowIndex == 0 && colIndex == 0)
				return Resources.ShiftCategory;
			if (rowIndex == 0 && colIndex == 1)
				return Resources.Limit;
			if (rowIndex == 0 && colIndex == 2)
				return Resources.PerPeriod;
			if (rowIndex == 0 && colIndex == 3)
				return Resources.PerWeek;
			if (rowIndex == 0 && colIndex == 4)
				return Resources.Max;
			return string.Empty;
		}

		public int? GetCurrentMaxNumberOf(IShiftCategory shiftCategory)
		{
			return CombinedLimitations[shiftCategory].MaxNumberOf;
		}

		public string OnQueryLimitationCellInfo(IShiftCategory shiftCategory, int colIndex)
		{
			if (colIndex == 1)
			{
				if (CombinedLimitations[shiftCategory].Limit.HasValue)
				{
					if (CombinedLimitations[shiftCategory].Limit.Value)
						return "true";
				}
				else
					return "";
			}
			else if (colIndex == 2)
			{
				if (CombinedLimitations[shiftCategory].Period.HasValue)
				{
					if (CombinedLimitations[shiftCategory].Period.Value)
						return "true";
				}
				else
					return "";
			}
			else if (colIndex == 3)
			{
				if (CombinedLimitations[shiftCategory].Weekly.HasValue)
				{
					if (CombinedLimitations[shiftCategory].Weekly.Value)
						return "true";
				}
				else
					return "";
			}

			return "false";
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "rowIndex-1")]
		public IShiftCategory OnQueryShiftCategoryCellInfo(int rowIndex)
		{
			if (ShiftCategories.Count == 0)
				return null;
			return ShiftCategories[rowIndex - 1];
		}

		public void AddLimitation(IShiftCategoryLimitation limitation)
		{
			foreach (var schedulePeriod in _schedulePeriods)
			{
				schedulePeriod.RemoveShiftCategoryLimitation(limitation.ShiftCategory);

				IShiftCategoryLimitation newLimitationInstance = new ShiftCategoryLimitation(limitation.ShiftCategory)
				{
					Weekly = limitation.Weekly,
					MaxNumberOf = limitation.MaxNumberOf
				};

				schedulePeriod.AddShiftCategoryLimitation(newLimitationInstance);
				_view.SetDirtySchedulePeriod(schedulePeriod);
			}
			CreateCombined();
		}

		public void RemoveLimitation(IShiftCategory shiftCategory)
		{
			foreach (var schedulePeriod in _schedulePeriods)
			{
				schedulePeriod.RemoveShiftCategoryLimitation(shiftCategory);
				_view.SetDirtySchedulePeriod(schedulePeriod);
			}
			CreateCombined();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void SetNumberOf(IShiftCategory shiftCategory, int value)
		{
			foreach (var schedulePeriod in _schedulePeriods)
			{
				foreach (var limitation in schedulePeriod.ShiftCategoryLimitationCollection())
				{
					if (limitation.ShiftCategory.Id == shiftCategory.Id)
					{
						limitation.MaxNumberOf = value;
						_view.SetDirtySchedulePeriod(schedulePeriod);
					}
				}
			}
			CreateCombined();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void SetWeekProperty(IShiftCategory shiftCategory, bool value)
		{
			foreach (var schedulePeriod in _schedulePeriods)
			{
				foreach (var limitation in schedulePeriod.ShiftCategoryLimitationCollection())
				{
					if (limitation.ShiftCategory.Id == shiftCategory.Id)
					{
						limitation.Weekly = value;
						_view.SetDirtySchedulePeriod(schedulePeriod);
					}
				}
			}
			CreateCombined();
		}

		public void Dispose()
		{
			_view = null;
			_combinedLimitations = null;
			_schedulePeriods = null;
			_shiftCategories = null;
		}
	}
}
