using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class BudgetGroup : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IBudgetGroup, IDeleteTag
    {
	    private readonly ISet<ISkill> _skillCollection;
        private string _timeZone;
        private int _daysPerYear;
        private string _name;
        private bool _isDeleted;
        private readonly IList<ICustomShrinkage> _customShrinkages = new List<ICustomShrinkage>();
        private readonly IList<ICustomEfficiencyShrinkage> _customEfficiencyShrinkages = new List<ICustomEfficiencyShrinkage>();

        public BudgetGroup()
        {
	        _skillCollection = new HashSet<ISkill>();
        }

        public virtual IEnumerable<ISkill> SkillCollection => _skillCollection;

	    public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual int DaysPerYear => _daysPerYear;

	    public virtual TimeZoneInfo TimeZone
        {
            get
            {
                if (String.IsNullOrEmpty(_timeZone)) return null;

                return TimeZoneInfo.FindSystemTimeZoneById(_timeZone);
            }
            set
            {
                InParameter.NotNull(nameof(TimeZone), value);
                _timeZone = value.Id;
            }
        }

        public virtual IEnumerable<ICustomShrinkage> CustomShrinkages => _customShrinkages;

	    public virtual IEnumerable<ICustomEfficiencyShrinkage> CustomEfficiencyShrinkages => _customEfficiencyShrinkages;

	    public virtual void AddSkill(ISkill skill)
        {
            _skillCollection.Add(skill);
        }

        public virtual bool IsDeleted => _isDeleted;

	    public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual void RemoveAllSkills()
        {
            _skillCollection.Clear();
        }

        public virtual void AddCustomShrinkage(ICustomShrinkage customShrinkage)
        {
            InParameter.NotNull(nameof(customShrinkage),customShrinkage);
            customShrinkage.SetParent(this);
            _customShrinkages.Add(customShrinkage);
        }

        public virtual void RemoveCustomShrinkage(ICustomShrinkage customShrinkage)
        {
            InParameter.NotNull(nameof(customShrinkage), customShrinkage);
            _customShrinkages.Remove(customShrinkage);
        }

        public virtual void AddCustomEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
        {
            InParameter.NotNull(nameof(customEfficiencyShrinkage), customEfficiencyShrinkage);
            customEfficiencyShrinkage.SetParent(this);
            _customEfficiencyShrinkages.Add(customEfficiencyShrinkage);
        }

        public virtual void RemoveCustomEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
        {
            InParameter.NotNull(nameof(customEfficiencyShrinkage), customEfficiencyShrinkage);
            _customEfficiencyShrinkages.Remove(customEfficiencyShrinkage);
        }

        public virtual bool IsCustomShrinkage(Guid customShrinkageId)
        {
            return _customShrinkages.Any(s => customShrinkageId == s.Id);
        }

        public virtual bool IsCustomEfficiencyShrinkage(Guid customEfficiencyShrinkageId)
        {
            return _customEfficiencyShrinkages.Any(s => customEfficiencyShrinkageId == s.Id);
        }

        public virtual void TrySetDaysPerYear(int daysPerYear)
        {
            _daysPerYear = daysPerYear;

            var maxDays = DaysInYear();
            if (_daysPerYear >= maxDays)
                _daysPerYear = maxDays;
            if (daysPerYear < 1)
                _daysPerYear = 1;
        }

        public virtual void UpdateCustomShrinkage(Guid id, ICustomShrinkage newCustomShrinkage)
        {
            InParameter.NotNull(nameof(newCustomShrinkage), newCustomShrinkage);
            var foundShrinkage = _customShrinkages.FirstOrDefault(s => s.Id.Equals(id));
            if (foundShrinkage != null)
            {
                foundShrinkage.ShrinkageName = newCustomShrinkage.ShrinkageName;
                foundShrinkage.IncludedInAllowance = newCustomShrinkage.IncludedInAllowance;
                foundShrinkage.RemoveAllAbsences();
                newCustomShrinkage.BudgetAbsenceCollection.ForEach(foundShrinkage.AddAbsence);
            }
        }

        public virtual void UpdateCustomEfficiencyShrinkage(Guid id, ICustomEfficiencyShrinkage newCustomEfficiencyShrinkage)
        {
            InParameter.NotNull(nameof(newCustomEfficiencyShrinkage), newCustomEfficiencyShrinkage);
            var foundShrinkage = _customEfficiencyShrinkages.FirstOrDefault(s => s.Id.Equals(id));
            if (foundShrinkage != null)
            {
                foundShrinkage.ShrinkageName = newCustomEfficiencyShrinkage.ShrinkageName;
                foundShrinkage.IncludedInAllowance = newCustomEfficiencyShrinkage.IncludedInAllowance;
            }
        }

        public virtual ICustomShrinkage GetShrinkage(Guid id)
        {
            return _customShrinkages.FirstOrDefault(s => s.Id.Equals(id));
        }

        public virtual ICustomEfficiencyShrinkage GetEfficiencyShrinkage(Guid id)
        {
            return _customEfficiencyShrinkages.FirstOrDefault(s => s.Id.Equals(id));
        }

        private static int DaysInYear()//Move to extentionMethod on DateTime?!?
        {
            return DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365;
        }
    }
}
