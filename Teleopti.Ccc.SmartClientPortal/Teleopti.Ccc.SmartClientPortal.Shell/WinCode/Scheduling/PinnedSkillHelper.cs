using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class PinnedSkillHelper
    {
        private TabControlAdv _tabControlAdv;
        private ISchedulingScreenSettings _currentSchedulingScreenSettings;
        private readonly IList<ISkill> _pinnedSkills = new List<ISkill>();
        private readonly IList<ISkill> _notPinnedSkills = new List<ISkill>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void InitialSetup(TabControlAdv tabControlAdv, ISchedulingScreenSettings currentSchedulingScreenSettings)
        {
            _tabControlAdv = tabControlAdv;
            _currentSchedulingScreenSettings = currentSchedulingScreenSettings;

            foreach (TabPageAdv tabPage in tabControlAdv.TabPages)
            {
                _notPinnedSkills.Add((ISkill)tabPage.Tag);
            }

            foreach (Guid t in currentSchedulingScreenSettings.PinnedSkillTabs)
            {
                var tabPage = findPageWithSkill(t, _tabControlAdv.TabPages);
                if (tabPage != null)
                {
                    var skill = tabPage.Tag as ISkill;
                    if (skill != null)
                    {
						if(!_pinnedSkills.Contains(skill))
							_pinnedSkills.Add(skill);
                        _notPinnedSkills.Remove(skill);
                    }
                }
            }

            foreach (var skill in pinnedSkillsCorrectSorted())
            {
                if(!skill.Id.HasValue)
                    continue;
                var tabPage = findPageWithSkill(skill.Id.Value, _tabControlAdv.TabPages);
                if (tabPage != null)
                {
                    _tabControlAdv.TabPages.Remove(tabPage);
                    _tabControlAdv.TabPages.Insert(pinnedSkillsCorrectSorted().IndexOf(skill), tabPage);
                    tabPage.Name = "Pinned";
                }
            }

            SortSkills();
        }

        public void SortSkills()
        {
			foreach (var skill in allSkillsCorrectSorted())
			{
				if (!skill.Id.HasValue)
					continue;
				var tabPage = findPageWithSkill(skill.Id.Value, _tabControlAdv.TabPages);
				if (tabPage != null)
				{
					var idx = allSkillsCorrectSorted().IndexOf(skill);
					if (idx != _tabControlAdv.TabPages.IndexOf(tabPage))
					{
						_tabControlAdv.TabPages.Remove(tabPage);
						_tabControlAdv.TabPages.Insert(idx, tabPage);
					}
				}
			}
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void PinSlashUnpinTab(TabPageAdv tabPageAdv)
        {
            var skill = tabPageAdv.Tag as ISkill;
            if (skill != null && skill.Id.HasValue)
            {
                if (_pinnedSkills.Contains(skill))
                {
                    _pinnedSkills.Remove(skill);
                    _notPinnedSkills.Add(skill);
                    _currentSchedulingScreenSettings.PinnedSkillTabs.Remove(skill.Id.Value);
                    tabPageAdv.Name = "";
                }
                else
                {
                    _notPinnedSkills.Remove(skill);
                    _pinnedSkills.Add(skill);
                    _currentSchedulingScreenSettings.PinnedSkillTabs.Add(skill.Id.Value);
                    tabPageAdv.Name = "Pinned";
                }
            }
            _tabControlAdv.TabPages.Remove(tabPageAdv);
            _tabControlAdv.TabPages.Insert(allSkillsCorrectSorted().IndexOf(skill), tabPageAdv);
        }

        public TabPageAdv PinnedPage()
        {

            if (_tabControlAdv != null && _tabControlAdv.TabPages.Count > 0)
            {
                return _tabControlAdv.TabPages[0];
            }
            return null;
        }


        private IEnumerable<ISkill> sortedPinnedVirtualSkills()
        {
            return _pinnedSkills.Where(skill => skill.IsVirtual).OrderBy(s => s.Name).ToList();
        }

        private IEnumerable<ISkill> sortedPinnedOrdinarySkills()
        {
            return _pinnedSkills.Where(skill => !skill.IsVirtual && skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).OrderBy(s => s.Name).ToList();
        }

        private IEnumerable<ISkill> sortedPinnedMaxSeatSkills()
        {
            return _pinnedSkills.Where(skill => !skill.IsVirtual && skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill).OrderBy(s => s.Name).ToList();
        }

        private IEnumerable<ISkill> sortedNotPinnedVirtualSkills()
        {
            return _notPinnedSkills.Where(skill => skill.IsVirtual).OrderBy(s => s.Name).ToList();
        }

        private IEnumerable<ISkill> sortedNotPinnedOrdinarySkills()
        {
            return _notPinnedSkills.Where(skill => !skill.IsVirtual && skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).OrderBy(s => s.Name).ToList();
        }

        private IEnumerable<ISkill> sortedNotPinnedMaxSeatSkills()
        {
            return _notPinnedSkills.Where(skill => !skill.IsVirtual && skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill).OrderBy(s => s.Name).ToList();
        }

        private IList<ISkill> pinnedSkillsCorrectSorted()
        {
            var ret = new List<ISkill>();
            ret.AddRange(sortedPinnedVirtualSkills());
            ret.AddRange(sortedPinnedOrdinarySkills());
            ret.AddRange(sortedPinnedMaxSeatSkills());
            return ret;
        }

        private IList<ISkill> allSkillsCorrectSorted()
        {
            var ret = new List<ISkill>();
            ret.AddRange(sortedPinnedVirtualSkills());
            ret.AddRange(sortedPinnedOrdinarySkills());
            ret.AddRange(sortedPinnedMaxSeatSkills());
            ret.AddRange(sortedNotPinnedVirtualSkills());
            ret.AddRange(sortedNotPinnedOrdinarySkills());
            ret.AddRange(sortedNotPinnedMaxSeatSkills());
            return ret;
        }
        private static TabPageAdv findPageWithSkill(Guid skillId, TabPageAdvCollection tabPages)
        {
            return (from TabPageAdv tabPageAdv in tabPages
                    let skill = tabPageAdv.Tag as ISkill
                    where skill != null && skill.Id.Equals(skillId)
                    select tabPageAdv).FirstOrDefault();
        }

        public void AddVirtualSkill(ISkill virtualSkill)
        {
            _notPinnedSkills.Add(virtualSkill);
        }

        public void RemoveVirtualSkill(ISkill virtualSkill)
        {
            _notPinnedSkills.Remove(virtualSkill);
            _pinnedSkills.Remove(virtualSkill);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void ReplaceOldWithNew(ISkill newSkill, ISkill oldSkill)
        {
            if (_notPinnedSkills.Contains(oldSkill))
            {
                _notPinnedSkills.Remove(oldSkill);
                _notPinnedSkills.Add(newSkill);
            }
            if (_pinnedSkills.Contains(oldSkill))
            {
                _pinnedSkills.Remove(oldSkill);
                if (oldSkill.Id.HasValue)
                    _currentSchedulingScreenSettings.PinnedSkillTabs.Remove(oldSkill.Id.Value);
                _pinnedSkills.Add(newSkill);
                if (newSkill.Id.HasValue)
                    _currentSchedulingScreenSettings.PinnedSkillTabs.Add(newSkill.Id.Value);
            }
        }

        public IList<ISkill> PinnedSkills { get { return _pinnedSkills; } }
        public IList<ISkill> NotPinnedSkills { get { return _notPinnedSkills; } }
    }
}