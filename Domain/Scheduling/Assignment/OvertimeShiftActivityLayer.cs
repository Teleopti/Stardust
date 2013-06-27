using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class OvertimeShiftActivityLayer : PersistedActivityLayer, IOvertimeShiftActivityLayer
    {
        private readonly IMultiplicatorDefinitionSet _definitionSet;

        public OvertimeShiftActivityLayer(IActivity activity, 
                                            DateTimePeriod period,
                                            IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
            : base(activity, period)
        {
            InParameter.NotNull("multiplicatorDefinitionSet", multiplicatorDefinitionSet);
            _definitionSet = multiplicatorDefinitionSet;
        }

        protected OvertimeShiftActivityLayer(){}

        public virtual IMultiplicatorDefinitionSet DefinitionSet
        {
            get { return _definitionSet; }
        }
    }
}
