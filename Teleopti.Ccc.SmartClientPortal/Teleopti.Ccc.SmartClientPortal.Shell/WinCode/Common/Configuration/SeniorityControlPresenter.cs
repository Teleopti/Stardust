using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class SeniorityControlPresenter
	{
		private readonly ISeniorityControlView _view;
		private ISeniorityWorkDayRanks _modelWorkDayRank;
		private IList<ISeniorityShiftCategoryRank> _modelShiftCategoryRanks; 
		private readonly ISeniorityWorkDayRanksRepository _repositoryWorkDayRanks;
		private readonly IRepository<IShiftCategory> _repositoryShiftCategory;

		public SeniorityControlPresenter(ISeniorityControlView view, ISeniorityWorkDayRanksRepository repositoryWorkDayRanks, IRepository<IShiftCategory> repositoryShiftCategory  )
		{
			_view = view;
			_repositoryWorkDayRanks = repositoryWorkDayRanks;
			_repositoryShiftCategory = repositoryShiftCategory;
		}

		public void Initialize()
		{
			createWorkDayRanksModel();
			createShiftCategoryRanksModel();
			_view.SetChangedInfo(_modelWorkDayRank);
		}

		private void createShiftCategoryRanksModel()
		{
			_modelShiftCategoryRanks = new List<ISeniorityShiftCategoryRank>();
			var shiftCategories = _repositoryShiftCategory.LoadAll().OrderBy(x => x.Rank.HasValue ? x.Rank : int.MaxValue).ThenBy(x=>x.Description.Name);
			var lastRank = 0;

			foreach (var shiftCategory in shiftCategories)
			{
				if (shiftCategory.Rank.HasValue) lastRank = shiftCategory.Rank.Value;
				else lastRank += 1;

				var item = new SeniorityShiftCategoryRank(shiftCategory) {Rank = lastRank};
				_modelShiftCategoryRanks.Add(item);	
			}
		}

		private void createWorkDayRanksModel()
		{
			var workDayRankings = _repositoryWorkDayRanks.LoadAll();
			
			for (var i = workDayRankings.Count -1; i > 0; i--)
			{
				_repositoryWorkDayRanks.Remove(workDayRankings[i]);
			}

			if (workDayRankings.IsEmpty())
			{
				_modelWorkDayRank = new SeniorityWorkDayRanks();
				_repositoryWorkDayRanks.Add(_modelWorkDayRank);
			}
			else
			{
				_modelWorkDayRank = workDayRankings.First();
			}

#pragma warning disable 618
			_repositoryWorkDayRanks.UnitOfWork.PersistAll();
#pragma warning restore 618
		}

		public IList<ISeniorityWorkDay> SeniorityWorkDays()
		{
			return _modelWorkDayRank == null ? new List<ISeniorityWorkDay>() : _modelWorkDayRank.SeniorityWorkDays();
		}

		public IList<ISeniorityShiftCategoryRank> SeniorityShiftCategoryRanks()
		{
			_modelShiftCategoryRanks = _modelShiftCategoryRanks.OrderBy(x => x.Rank).ToList();
			return _modelShiftCategoryRanks;
		}

		public void Persist()
		{
			_repositoryWorkDayRanks.Remove(_modelWorkDayRank);
			_modelWorkDayRank.ClearId();
			_repositoryWorkDayRanks.Add(_modelWorkDayRank);
#pragma warning disable 618
			_repositoryWorkDayRanks.UnitOfWork.PersistAll();
#pragma warning restore 618

			foreach (var seniorityShiftCategoryRank in SeniorityShiftCategoryRanks())
			{
				seniorityShiftCategoryRank.ShiftCategory.Rank = seniorityShiftCategoryRank.Rank;
#pragma warning disable 618
				_repositoryWorkDayRanks.UnitOfWork.Merge(seniorityShiftCategoryRank.ShiftCategory);
#pragma warning restore 618
			}

#pragma warning disable 618
			_repositoryWorkDayRanks.UnitOfWork.PersistAll();
#pragma warning restore 618
		}

		public void Unload()
		{
			_modelWorkDayRank = null;
			_modelShiftCategoryRanks.Clear();
		}

		public void MoveTopShiftCategory(int index)
		{
			for (var i = index; i >= 0; i--)
			{
				if (i.Equals(index)) _modelShiftCategoryRanks[i].Rank = 1;
				else _modelShiftCategoryRanks[i].Rank += 1;
			}

			_view.RefreshListBoxShiftCategoryRank(0);
		}

		public void MoveBottomShiftCategory(int index)
		{
			for (var i = index; i < _modelShiftCategoryRanks.Count; i++)
			{
				if (i.Equals(index)) _modelShiftCategoryRanks[i].Rank = _modelShiftCategoryRanks.Last().Rank;
				else _modelShiftCategoryRanks[i].Rank -= 1;
			}

			_view.RefreshListBoxShiftCategoryRank(_modelShiftCategoryRanks.Count -1);
		}

		public void MoveUpShiftCategory(int index)
		{
			if (index == 0) return;

			var currentRank = _modelShiftCategoryRanks[index].Rank;
			_modelShiftCategoryRanks[index].Rank = _modelShiftCategoryRanks[index - 1].Rank;
			_modelShiftCategoryRanks[index - 1].Rank = currentRank;
			
			_view.RefreshListBoxShiftCategoryRank(index - 1);
		}

		public void MoveDownShiftCategory(int index)
		{
			if (index >= _modelShiftCategoryRanks.Count -1) return;

			var currentRank = _modelShiftCategoryRanks[index].Rank;
			_modelShiftCategoryRanks[index].Rank = _modelShiftCategoryRanks[index + 1].Rank;
			_modelShiftCategoryRanks[index + 1].Rank = currentRank;


			_view.RefreshListBoxShiftCategoryRank(index + 1);
		}

		public void MoveTopWorkDay(int index)
		{
			var currentOrder = _modelWorkDayRank.SeniorityWorkDays();

			for (var i = index; i >=0 ; i--)
			{
				var item = currentOrder[i];
				setRankWorkDay(item, i.Equals(index) ? 1 : item.Rank + 1);
			}

			_view.RefreshListBoxWorkingDays(0);
		}

		public void MoveBottomWorkDay(int index)
		{
			var currentOrder = _modelWorkDayRank.SeniorityWorkDays();

			for (var i = index; i < 7; i++)
			{
				var item = currentOrder[i];
				setRankWorkDay(item, i.Equals(index) ? 7 : item.Rank - 1);
			}

			_view.RefreshListBoxWorkingDays(6);
		}

		public void MoveUpWorkDay(int index)
		{
			if (index == 0) return;

			var currentOrder = _modelWorkDayRank.SeniorityWorkDays();

			var item = currentOrder[index];
			var itemBefore = currentOrder[index - 1];
			var currentRank = item.Rank;

			setRankWorkDay(item, currentRank - 1);
			setRankWorkDay(itemBefore, currentRank);

			_view.RefreshListBoxWorkingDays(index -1);
		}

		public void MoveDownWorkDay(int index)
		{
			if (index == 6) return;

			var currentOrder = _modelWorkDayRank.SeniorityWorkDays();

			var item = currentOrder[index];
			var itemAfter = currentOrder[index + 1];
			var currentRank = item.Rank;

			setRankWorkDay(item, currentRank + 1);
			setRankWorkDay(itemAfter, currentRank);

			_view.RefreshListBoxWorkingDays(index + 1);
		}

		private void setRankWorkDay(ISeniorityWorkDay seniorityWorkDay, int rank)
		{
			switch (seniorityWorkDay.DayOfWeek)
			{
				case DayOfWeek.Monday:_modelWorkDayRank.Monday = rank; break;
				case DayOfWeek.Tuesday: _modelWorkDayRank.Tuesday = rank; break;
				case DayOfWeek.Wednesday: _modelWorkDayRank.Wednesday = rank; break;
				case DayOfWeek.Thursday: _modelWorkDayRank.Thursday = rank; break;
				case DayOfWeek.Friday: _modelWorkDayRank.Friday = rank; break;
				case DayOfWeek.Saturday: _modelWorkDayRank.Saturday = rank; break;
				case DayOfWeek.Sunday: _modelWorkDayRank.Sunday = rank; break;
			}
		}
	}
}
