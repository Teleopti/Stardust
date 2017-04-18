using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models
{
    public abstract class BaseModel : IBaseModel
    {
        private readonly IWorkShiftRuleSet _workShiftRuleSet;

        protected BaseModel(IWorkShiftRuleSet workShiftRuleSet)
        {
            _workShiftRuleSet = workShiftRuleSet;
        }

        public IWorkShiftRuleSet WorkShiftRuleSet
        {
            get { return _workShiftRuleSet; }
        }

        public Description WorkShiftRuleSetName
        {
            get
            {
                return _workShiftRuleSet.Description;
            } 
            set
            {
                _workShiftRuleSet.Description = value;
            }
        }

        public virtual bool Validate()
        {
            return true;
        }
    }


    public abstract class BaseModel<T> : BaseModel, IBaseModel<T>
    {
        private readonly T _containedEntity;

        protected BaseModel(IWorkShiftRuleSet workShiftRuleSet, T containedEntity)
            :base(workShiftRuleSet)
        {
            _containedEntity = containedEntity;
        }

        public T ContainedEntity
        {
            get { return _containedEntity; }
        }


    }
}
