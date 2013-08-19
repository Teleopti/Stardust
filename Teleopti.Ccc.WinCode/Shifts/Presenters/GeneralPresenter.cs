using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.WinCode.Shifts.Presenters
{
    public class GeneralPresenter : BasePresenter, IGeneralPresenter
    {
        private readonly IDictionary<ShiftCreatorViewType, IPresenterBase> _presenters;

        private readonly IGeneralTemplatePresenter _generalTemplatePresenter;
        private readonly IActivityPresenter _activityPresenter;
        private readonly IAccessibilityDatePresenter _accessibilityDatePresenter;
        private readonly IActivityTimeLimiterPresenter _activityTimeLimiterPresenter;
        private readonly IDaysOfWeekPresenter _daysOfWeekPresenter;

        public GeneralPresenter(IExplorerPresenter explorer, IDataHelper dataHelper)
            : base(explorer,dataHelper)
        {
            _presenters = new Dictionary<ShiftCreatorViewType, IPresenterBase>();

            _generalTemplatePresenter = new GeneralTemplatePresenter(explorer,dataHelper);
            _activityPresenter = new ActivityPresenter(explorer,dataHelper);
            _accessibilityDatePresenter = new AccessibilityDatePresenter(explorer,dataHelper);
            _activityTimeLimiterPresenter = new ActivityTimeLimiterPresenter(explorer,dataHelper);
            _daysOfWeekPresenter = new DaysOfWeekPresenter(explorer,dataHelper);

            _presenters.Add(ShiftCreatorViewType.General, _generalTemplatePresenter);
            _presenters.Add(ShiftCreatorViewType.Activities, _activityPresenter);
            _presenters.Add(ShiftCreatorViewType.DateExclusion, _accessibilityDatePresenter);
            _presenters.Add(ShiftCreatorViewType.Limitation, _activityTimeLimiterPresenter);
            _presenters.Add(ShiftCreatorViewType.WeekdayExclusion, _daysOfWeekPresenter);
        }

        public IGeneralTemplatePresenter GeneralTemplatePresenter
        {
            get { return (IGeneralTemplatePresenter)_presenters[ShiftCreatorViewType.General]; }
        }

        public IActivityPresenter ActivityPresenter
        {
            get { return (IActivityPresenter)_presenters[ShiftCreatorViewType.Activities]; }
        }

        public IAccessibilityDatePresenter AccessibilityDatePresenter
        {
            get { return (IAccessibilityDatePresenter)_presenters[ShiftCreatorViewType.DateExclusion]; }
        }

        public IActivityTimeLimiterPresenter ActivityTimeLimiterPresenter
        {
            get { return (IActivityTimeLimiterPresenter)_presenters[ShiftCreatorViewType.Limitation]; }
        }

        public IDaysOfWeekPresenter DaysOfWeekPresenter
        {
            get { return (IDaysOfWeekPresenter)_presenters[ShiftCreatorViewType.WeekdayExclusion]; }
        }

        public override void LoadModelCollection()
        {
            foreach (KeyValuePair<ShiftCreatorViewType, IPresenterBase> pair in _presenters)
            {
                pair.Value.LoadModelCollection();
            }
        }

        public new bool Validate()
        {
            foreach (KeyValuePair<ShiftCreatorViewType, IPresenterBase> pair in _presenters)
            {
                if (!pair.Value.Validate())
                    return false;
            }

            return true;
        }
    }
}
