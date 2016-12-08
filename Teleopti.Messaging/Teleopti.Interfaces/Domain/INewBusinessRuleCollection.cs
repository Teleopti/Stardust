using System;
using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    public interface INewBusinessRuleCollection : ICollection<INewBusinessRule>
    {
        IEnumerable<IBusinessRuleResponse> CheckRules(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays);

        void DoNotHaltModify(IBusinessRuleResponse businessRuleResponseToOverride);

        void DoNotHaltModify(Type businessRuleType);

        INewBusinessRule Item(Type businessRuleType);

        void SetUICulture(CultureInfo cultureInfo);

        CultureInfo UICulture { get; }
    }
}