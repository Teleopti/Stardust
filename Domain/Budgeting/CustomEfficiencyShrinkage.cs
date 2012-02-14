using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class CustomEfficiencyShrinkage : AggregateEntity, ICustomEfficiencyShrinkage
    {
        private string _shrinkageName;
        private int _orderIndex;
        private bool _includedInAllowance;

        protected CustomEfficiencyShrinkage()
        {
        }

        public CustomEfficiencyShrinkage(string shrinkageName)
            : this()
        {
            _shrinkageName = shrinkageName;
        }

        public CustomEfficiencyShrinkage(string shrinkageName, bool includedInAllowance)
            : this()
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
                IBudgetGroup owner = Parent as IBudgetGroup;
                _orderIndex = owner == null ? -1 : ((IList<ICustomEfficiencyShrinkage>)owner.CustomEfficiencyShrinkages).IndexOf(this);
                return _orderIndex;
            }
            set
            {
                _orderIndex = value;
            }
        }
    }
}