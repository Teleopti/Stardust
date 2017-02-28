using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class VirtualSkillHelper : IVirtualSkillHelper
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;

		private const string SettingName = "VirtualSkillSetting";

		private readonly VirtualSkillSetting _defaultSetting = new VirtualSkillSetting();
		private VirtualSkillSetting _currentSetting = new VirtualSkillSetting();

		public VirtualSkillHelper(IUnitOfWorkFactory unitOfWorkFactory, IPersonalSettingDataRepository personalSettingDataRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personalSettingDataRepository = personalSettingDataRepository;
		}

		public void SaveVirtualSkill(IAggregateSkill virtualSkill)
		{
		    var skill = virtualSkill as ISkill;
			if(skill == null) return;
			string name = skill.Name;
			if (virtualSkill.AggregateSkills.Count == 0)
			{
				removeVirtualSkill(name);
				persistCurrentSetting();
				return;
			}

			VirtualSkill vSkill;
			if (!_currentSetting.VirtualSkills.TryGetValue(name, out vSkill))
			{
				var id = Guid.NewGuid();
				if (skill.Id.HasValue)
					id = skill.Id.Value;
				vSkill = addVirtualSkill(name, id);
			}

			updateVirtualSkill(vSkill, virtualSkill);
			
			persistCurrentSetting();
		}

		public void EditAndRenameVirtualSkill(IAggregateSkill newVirtualSkill, string oldName)
		{
			removeVirtualSkill(oldName);
			var newSkill = newVirtualSkill as ISkill;
			if(newSkill == null)
				return;
			string name = newSkill.Name;
			VirtualSkill vSkill;
			if (!_currentSetting.VirtualSkills.TryGetValue(name, out vSkill))
			{
				var id = Guid.NewGuid();
				if (newSkill.Id.HasValue)
					id = newSkill.Id.Value;
				vSkill = addVirtualSkill(name, id);
			}

			updateVirtualSkill(vSkill, newVirtualSkill);

			persistCurrentSetting();
		}

		public IList<ISkill> LoadVirtualSkills(IList<ISkill> availableSkills)
		{
			IList<ISkill> returnList = new List<ISkill>();
			if (availableSkills.Count == 0) return returnList;

			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_currentSetting = _personalSettingDataRepository.FindValueByKey(SettingName, _defaultSetting);
			}

			foreach (VirtualSkill virtualSkill in _currentSetting.VirtualSkills.Values)
			{
				ISkill skill = new Skill(virtualSkill.Name, virtualSkill.Name, Color.Pink, 15, new SkillTypePhone(new Description("Aggregate"), ForecastSource.InboundTelephony));
				if(!virtualSkill.Id.Equals(Guid.Empty))
					skill.SetId(virtualSkill.Id);

				foreach (Guid guid in virtualSkill.ChildSkills)
				{
					var availableSkill = availableSkills.FirstOrDefault(s => s.Id.GetValueOrDefault() == guid);
					if (availableSkill != null)
					{
						skill.AddAggregateSkill(availableSkill);
						skill.TimeZone = availableSkill.TimeZone;
					}
				}
				if (skill.AggregateSkills.Count > 0)
				{
					skill.DefaultResolution = skill.AggregateSkills.Min(s => s.DefaultResolution);
					skill.IsVirtual = true;
					returnList.Add(skill);
				}
			}
			return returnList;
		}

		private void persistCurrentSetting()
		{
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_personalSettingDataRepository.PersistSettingValue(_currentSetting);
				uow.PersistAll();
			}
		}

		private static void updateVirtualSkill(VirtualSkill vSkill, IAggregateSkill aggregateSkill)
		{
			vSkill.ChildSkills.Clear();
			foreach (ISkill skill in aggregateSkill.AggregateSkills)
			{
				vSkill.ChildSkills.Add(skill.Id.GetValueOrDefault());
			}
		}

		private VirtualSkill addVirtualSkill(string key, Guid newId)
		{
			var vSkill = new VirtualSkill { Name = key, Id = newId };
			_currentSetting.VirtualSkills.Add(key, vSkill);

			return vSkill;
		}

		private void removeVirtualSkill(string key)
		{
			VirtualSkill virtualSkill;
			if (_currentSetting.VirtualSkills.TryGetValue(key, out virtualSkill))
				_currentSetting.VirtualSkills.Remove(key);
		}
	}
}
