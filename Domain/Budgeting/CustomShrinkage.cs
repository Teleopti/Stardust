using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class CustomShrinkage : AggregateEntity, ICustomShrinkage
    {
        private string _shrinkageName;
        private int _orderIndex;
        private bool _includedInAllowance;
	    private readonly ISet<IAbsence> _budgetAbsenceCollection;

        protected CustomShrinkage()
        {
            _budgetAbsenceCollection = new HashSet<IAbsence>();
        }

        public CustomShrinkage(string shrinkageName) : this()
        {
            _shrinkageName = shrinkageName;
            _includedInAllowance = false;
        }

        public CustomShrinkage(string shrinkageName, bool includedInAllowance) : this()
        {
            _shrinkageName = shrinkageName;
            _includedInAllowance = includedInAllowance;
        }

        public virtual string ShrinkageName
        {
            get { return _shrinkageName; }
            set { _shrinkageName = value; }
        }

        public virtual bool IncludedInAllowance
        {
            get { return _includedInAllowance; }
            set { _includedInAllowance = value; }
        }

        public virtual int OrderIndex
        {
            get
            {
                var owner = Parent as IBudgetGroup;
                _orderIndex = owner == null ? -1 : ((IList<ICustomShrinkage>)owner.CustomShrinkages).IndexOf(this);
                return _orderIndex;
            }
            set
            {
                _orderIndex = value;
            }
        }

        public virtual IEnumerable<IAbsence> BudgetAbsenceCollection
        {
            get { return _budgetAbsenceCollection; }
        }

        public virtual void AddAbsence(IAbsence absence)
        {
            _budgetAbsenceCollection.Add(absence);
        }

        public virtual void RemoveAllAbsences()
        {
            _budgetAbsenceCollection.Clear();
        }
    }
}