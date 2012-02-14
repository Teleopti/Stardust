using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class PreferenceRestriction : RestrictionBase, IPreferenceRestriction
    {
        private IAbsence _absence;
        private IShiftCategory _shiftCategory;
        private IDayOffTemplate _dayOffTemplate;
        private IList<IActivityRestriction> _activityRestrictionCollection = new List<IActivityRestriction>();
        private bool _mustHave;

        public virtual IAbsence Absence
        {
            get { return _absence; }
            set { _absence = value; }
        }

        public virtual IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
            set { _shiftCategory = value; }
        }

        public virtual IDayOffTemplate DayOffTemplate
        {
            get { return _dayOffTemplate; }
            set { _dayOffTemplate = value; }
        }

        public virtual ReadOnlyCollection<IActivityRestriction> ActivityRestrictionCollection
        {
            get { return new ReadOnlyCollection<IActivityRestriction>(_activityRestrictionCollection); }
        }

        public virtual void AddActivityRestriction(IActivityRestriction activityRestriction)
        {
            activityRestriction.SetParent(this);
            _activityRestrictionCollection.Add(activityRestriction);
        }

        public virtual void RemoveActivityRestriction(IActivityRestriction activityRestriction)
        {
            _activityRestrictionCollection.Remove(activityRestriction);
        }

        public override bool IsRestriction()
        {
            if (_absence != null)
                return true;

            if (_shiftCategory != null)
                return true;

            if (_dayOffTemplate != null)
                return true;

            if(_activityRestrictionCollection.Count > 0)
                return true;

            return base.IsRestriction();
        }

        public override object Clone()
        {
            PreferenceRestriction ret = (PreferenceRestriction)MemberwiseClone();
            ret._activityRestrictionCollection = new List<IActivityRestriction>();
            foreach (ActivityRestriction activityRestriction in _activityRestrictionCollection)
            {
                ret.AddActivityRestriction((ActivityRestriction)activityRestriction.Clone());
            }
            
            return ret;
        }

        public virtual IPreferenceRestriction NoneEntityClone()
        {
            IPreferenceRestriction ret = (IPreferenceRestriction)Clone();
            foreach (IActivityRestriction activityRestriction in ret.ActivityRestrictionCollection)
            {
                activityRestriction.SetId(null);
            }
            ret.SetId(null);
            return ret;
        }

        public virtual IPreferenceRestriction EntityClone()
        {
            return (IPreferenceRestriction)Clone();
        }
       
        public virtual bool MustHave
        {
            get { return _mustHave; }
            set { _mustHave = value; }
        }
     }
}