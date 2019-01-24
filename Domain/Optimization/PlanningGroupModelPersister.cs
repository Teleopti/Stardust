using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupModelPersister : IPlanningGroupModelPersister
	{
		private readonly IPlanningGroupRepository _planningGroupRepository;
		private readonly FilterMapper _filterMapper;
		private readonly IPlanningGroupSettingsModelPersister _planningGroupSettingsModelPersister;

		public PlanningGroupModelPersister(IPlanningGroupRepository planningGroupRepository, FilterMapper filterMapper, IPlanningGroupSettingsModelPersister planningGroupSettingsModelPersister)
		{
			_planningGroupRepository = planningGroupRepository;
			_filterMapper = filterMapper;
			_planningGroupSettingsModelPersister = planningGroupSettingsModelPersister;
		}

		public void Persist(PlanningGroupModel planningGroupModel)
		{
			if (planningGroupModel.Id == Guid.Empty)
			{
				var planningGroup = new PlanningGroup();
				setProperties(planningGroup, planningGroupModel);
				_planningGroupRepository.Add(planningGroup);
				setPlanningGroupSettings(planningGroup, planningGroupModel);
			}
			else
			{
				var planningGroup = _planningGroupRepository.Get(planningGroupModel.Id);
				setProperties(planningGroup, planningGroupModel);
				setPlanningGroupSettings(planningGroup, planningGroupModel);
			}
		}

		private void setProperties(PlanningGroup planningGroup, PlanningGroupModel planningGroupModel)
		{
			planningGroup.Name = planningGroupModel.Name;
			planningGroup.SetGlobalValues(new Percent(planningGroupModel.PreferencePercent / 100d));
			planningGroup.SetTeamSettings(planningGroupModel.TeamSettings);

			planningGroup.ClearFilters();
			foreach (var filter in planningGroupModel.Filters.Select(filterModel => _filterMapper.ToEntity(filterModel)))
			{
				planningGroup.AddFilter(filter);
			}
		}

		private void setPlanningGroupSettings(PlanningGroup planningGroup, PlanningGroupModel planningGroupModel)
		{
			if (!planningGroupModel.Settings.IsNullOrEmpty())
			{
				foreach (var setting in planningGroupModel.Settings)
				{
					if (setting.Default)
					{
						_planningGroupSettingsModelPersister.UpdateDefaultSetting(planningGroup.Settings.Single(x => x.Default), setting);
					}
					else
					{
						setting.PlanningGroupId = planningGroup.Id.Value;
						_planningGroupSettingsModelPersister.Persist(setting);
					}
				}
			}
		}

		public void Delete(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null) return;
			_planningGroupRepository.Remove(planningGroup);
		}
	}
}