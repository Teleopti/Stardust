using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffRulesModelPersister : IDayOffRulesModelPersister
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly FilterMapper _filterMapper;

		public DayOffRulesModelPersister(IDayOffRulesRepository dayOffRulesRepository, FilterMapper filterMapper)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
			_filterMapper = filterMapper;
		}

		public void Persist(DayOffRulesModel model)
		{
			if (model.Id == Guid.Empty)
			{
				var dayOffRules = model.Default ?
					DayOffRules.CreateDefault() :
					new DayOffRules();
				setProperies(dayOffRules, model);
				_dayOffRulesRepository.Add(dayOffRules);
			}
			else
			{
				var dayOffRules = _dayOffRulesRepository.Get(model.Id);
				setProperies(dayOffRules, model);
			}
		}

		public void Delete(Guid id)
		{
			var dayOffRule = _dayOffRulesRepository.Get(id);
			_dayOffRulesRepository.Remove(dayOffRule);
		}

		private void setProperies(DayOffRules dayOffRules, DayOffRulesModel dayOffRulesModel)
		{
			dayOffRules.DayOffsPerWeek = new MinMax<int>(dayOffRulesModel.MinDayOffsPerWeek, dayOffRulesModel.MaxDayOffsPerWeek);
			dayOffRules.ConsecutiveDayOffs = new MinMax<int>(dayOffRulesModel.MinConsecutiveDayOffs, dayOffRulesModel.MaxConsecutiveDayOffs);
			dayOffRules.ConsecutiveWorkdays = new MinMax<int>(dayOffRulesModel.MinConsecutiveWorkdays, dayOffRulesModel.MaxConsecutiveWorkdays);
			dayOffRules.Name = dayOffRulesModel.Name;

			dayOffRules.ClearFilters();
			foreach (var filter in dayOffRulesModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				dayOffRules.AddFilter(filter);
			}
		}
	}
}