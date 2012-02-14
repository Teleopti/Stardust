using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{

    public abstract class BusinessRule : IBusinessRule
    {

        public abstract bool Validate(ISchedulePart currentCompletePart, DateTime dateToStartCheckOn, CultureInfo cultureInfo);
        public string ErrorMessage { get; protected set; }
        public abstract bool IsMandatory { get; }
    }
}
