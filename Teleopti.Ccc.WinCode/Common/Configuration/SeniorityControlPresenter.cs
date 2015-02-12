using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class SeniorityControlPresenter
	{
		private readonly ISeniorityControlView _view;
		private ISeniorityWorkDayRanks _model;
		private readonly IRepository<ISeniorityWorkDayRanks> _repository;

		public SeniorityControlPresenter(ISeniorityControlView view, IRepository<ISeniorityWorkDayRanks> repository)
		{
			_view = view;
			_repository = repository;
		}

		public void Initialize()
		{
			var workDayRankings = _repository.LoadAll();
			_model = !workDayRankings.IsEmpty() ? workDayRankings.First() : new SeniorityWorkDayRanks();
		}

		public IList<ISeniorityWorkDay> SeniorityWorkDays()
		{
			return _model == null ? new List<ISeniorityWorkDay>() : _model.SeniorityWorkDays();
		}

		public void Persist()
		{
			if (!_model.Id.HasValue) _repository.Add(_model);
			else _repository.UnitOfWork.Merge(_model);
			_repository.UnitOfWork.PersistAll();
		}

		public void Unload()
		{
			_model = null;
		}

		public void MoveTop(int index)
		{
			var currentOrder = _model.SeniorityWorkDays();

			for (var i = index; i >=0 ; i--)
			{
				var item = currentOrder[i];
				setRank(item, i.Equals(index) ? 1 : item.Rank + 1);
			}

			_view.RefreshListBoxWorkingDays(0);
		}

		public void MoveBottom(int index)
		{
			var currentOrder = _model.SeniorityWorkDays();

			for (var i = index; i < 7; i++)
			{
				var item = currentOrder[i];
				setRank(item, i.Equals(index) ? 7 : item.Rank - 1);
			}

			_view.RefreshListBoxWorkingDays(6);
		}

		public void MoveUp(int index)
		{
			if (index == 0) return;

			var currentOrder = _model.SeniorityWorkDays();

			var item = currentOrder[index];
			var itemBefore = currentOrder[index - 1];
			var currentRank = item.Rank;

			setRank(item, currentRank - 1);
			setRank(itemBefore, currentRank);

			_view.RefreshListBoxWorkingDays(index -1);
		}

		public void MoveDown(int index)
		{
			if (index == 6) return;

			var currentOrder = _model.SeniorityWorkDays();

			var item = currentOrder[index];
			var itemAfter = currentOrder[index + 1];
			var currentRank = item.Rank;

			setRank(item, currentRank + 1);
			setRank(itemAfter, currentRank);

			_view.RefreshListBoxWorkingDays(index + 1);
		}

		private void setRank(ISeniorityWorkDay seniorityWorkDay, int rank)
		{
			switch (seniorityWorkDay.DayOfWeek)
			{
				case DayOfWeek.Monday:_model.Monday = rank; break;
				case DayOfWeek.Tuesday: _model.Tuesday = rank; break;
				case DayOfWeek.Wednesday: _model.Wednesday = rank; break;
				case DayOfWeek.Thursday: _model.Thursday = rank; break;
				case DayOfWeek.Friday: _model.Friday = rank; break;
				case DayOfWeek.Saturday: _model.Saturday = rank; break;
				case DayOfWeek.Sunday: _model.Sunday = rank; break;
			}
		}
	}
}
