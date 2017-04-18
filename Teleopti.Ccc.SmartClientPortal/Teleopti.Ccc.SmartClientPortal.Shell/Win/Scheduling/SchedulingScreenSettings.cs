using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public enum SkillResultViewSetting
	{
		Period,
		Month,
		Week,
		Day,
		Intraday
	}
    [Serializable]
    public class SchedulingScreenSettings : SettingValue, ISchedulingScreenSettings
    {
        private readonly IList<string> _quickAccessButtons = new List<string>();
        private TimeSpan _editorSnapToResolution;
        //Negations because default values did not work as expected
        private bool _hideEditor;
        private bool _hideGraph;
        private bool _hideResult;
	    private bool _hideInfoPanel;
        private bool _hideRibbonTexts;
        private Guid? _defaultScheduleTag;
		private SkillResultViewSetting _skillResultViewSetting = SkillResultViewSetting.Day;
		private SchedulerSortCommandSetting _sortCommandSetting = SchedulerSortCommandSetting.NoSortCommand;

        private IList<Guid> _pinnedSkills;

		public SchedulerSortCommandSetting SortCommandSetting
    	{
			get { return _sortCommandSetting; }
			set { _sortCommandSetting = value; }
    	}

        public Guid? DefaultScheduleTag
        {
            get { return _defaultScheduleTag; }
            set { _defaultScheduleTag = value; }
        }

        public SchedulingScreenSettings()
        {
            _quickAccessButtons.Add("NotSavedBefore");
            EditorSnapToResolution = TimeSpan.FromMinutes(15);
        }

        public IList<string> QuickAccessButtons
        {
            get { return _quickAccessButtons; }
        }

        public TimeSpan EditorSnapToResolution
        {
            get { return _editorSnapToResolution; }
            set { _editorSnapToResolution = value; }
        }

        public bool HideEditor
        {
            get { return _hideEditor; }
            set { _hideEditor = value; }
        }

        public bool HideGraph
        {
            get { return _hideGraph; }
            set { _hideGraph = value; }
        }

        public bool HideResult
        {
            get { return _hideResult; }
            set { _hideResult = value; }
        }

	    public bool HideInfoPanel
	    {
			get { return _hideInfoPanel; }
			set { _hideInfoPanel = value; }
	    }

        public bool HideRibbonTexts
        {
            get { return _hideRibbonTexts; }
            set { _hideRibbonTexts = value; }
        }

        public IList<Guid> PinnedSkillTabs
        {
            get
            {
                if(_pinnedSkills == null)
                    _pinnedSkills = new List<Guid>();
                return _pinnedSkills;
            }
        }

    	public SkillResultViewSetting SkillResultViewSetting
    	{
			get { return _skillResultViewSetting; }
			set { _skillResultViewSetting = value; }
    	}
    }
}