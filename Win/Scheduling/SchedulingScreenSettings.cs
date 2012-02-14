using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.Win.Scheduling
{
    
    [Serializable]
    public class SchedulingScreenSettings : SettingValue, ISchedulingScreenSettings
    {
        private readonly IList<string> _quickAccessButtons = new List<string>();
        private TimeSpan _editorSnapToResolution;
        //Negations because default values did not work as expected
        private bool _hideEditor;
        private bool _hideGraph;
        private bool _hideResult;
        private bool _hideRibbonTexts;
        private Guid? _defaultScheduleTag;
        private OptimizeActivitiesSettings _optimizeActivitiesSettings = new OptimizeActivitiesSettings();
        private string _selectedFairnessGroupingKey;
        private IList<Guid> _pinnedSkills;

        public Guid? DefaultScheduleTag
        {
            get { return _defaultScheduleTag; }
            set { _defaultScheduleTag = value; }
        }

        public OptimizeActivitiesSettings OptimizeActivitiesSettings
        {
            get { return _optimizeActivitiesSettings; }
            set { _optimizeActivitiesSettings = value; }
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

        public bool HideRibbonTexts
        {
            get { return _hideRibbonTexts; }
            set { _hideRibbonTexts = value; }
        }

        public string SelectedFairnessGroupingKey
        {
            get { return _selectedFairnessGroupingKey; }
            set { _selectedFairnessGroupingKey = value; }
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
    }
}