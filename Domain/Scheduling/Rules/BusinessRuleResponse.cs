using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class BusinessRuleResponse : IBusinessRuleResponse
    {
        private readonly Type _typeOfRule;
        private readonly string _message;
        private readonly bool _error;
        private readonly bool _mandatory;
        private readonly DateTimePeriod _period;
        private readonly IPerson _person;
        private DateOnlyPeriod _dateOnlyPeriod;

        public BusinessRuleResponse(Type typeOfRule, string message, bool error, bool mandatory, DateTimePeriod period, IPerson person, DateOnlyPeriod dateOnlyPeriod)
        {
            //if(person.GetType()!=typeof(Person))
            //{
            //    string foo = string.Empty;
            //}

            InParameter.NotNull("person", person);
            _typeOfRule = typeOfRule;
            _message = message ?? string.Empty;
            _error = error;
            _mandatory = mandatory;
            _period = period;
            _person = person;
            _dateOnlyPeriod = dateOnlyPeriod;
        }

        public Type TypeOfRule
        {
            get { return _typeOfRule; }
        }

        public string Message
        {
            get { return _message; }
        }

        public bool Error
        {
            get { return _error; }
        }

        public bool Overridden { get; set; }

        public bool Mandatory
        {
            get { return _mandatory; }
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }

        public IPerson Person
        {
            get { return _person; }
        }

        public override bool Equals(object obj)
        {
            var casted = obj as BusinessRuleResponse;
            if (obj == null || casted == null)
            {
                return false;
            }

            return (casted._person.Equals(_person) && _typeOfRule == casted._typeOfRule && casted._period.Equals(_period) && casted._dateOnlyPeriod.Equals(_dateOnlyPeriod));
        }

        public override int GetHashCode()
        {
            return (_person.GetHashCode() ^ _typeOfRule.GetHashCode() ^ _period.GetHashCode() ^ _dateOnlyPeriod.GetHashCode());
        }
    }
}