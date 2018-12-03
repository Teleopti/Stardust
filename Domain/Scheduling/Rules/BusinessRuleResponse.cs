using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

        public BusinessRuleResponse(Type typeOfRule, string message, bool error, bool mandatory, DateTimePeriod period, IPerson person, DateOnlyPeriod dateOnlyPeriod, string friendlyName)
        {
            InParameter.NotNull(nameof(person), person);
            _typeOfRule = typeOfRule;
            _message = message ?? string.Empty;
            _error = error;
            _mandatory = mandatory;
            _period = period;
            _person = person;
            _dateOnlyPeriod = dateOnlyPeriod;
	        FriendlyName = friendlyName;
        }

		public BusinessRuleResponse(string message, bool mandatory = false)
		{
			_typeOfRule = typeof(BusinessRuleResponse);
			_message = message ?? string.Empty;
			_mandatory = mandatory;
		}

		public Type TypeOfRule => _typeOfRule;

	    public string Message => _message;

	    public bool Error => _error;

	    public bool Overridden { get; set; }

        public bool Mandatory => _mandatory;

	    public DateTimePeriod Period => _period;

	    public DateOnlyPeriod DateOnlyPeriod => _dateOnlyPeriod;

	    public IPerson Person => _person;

	    public string FriendlyName { get; }

	    public override bool Equals(object obj)
		{
			return obj != null && obj is BusinessRuleResponse casted &&
				   (casted._person.Equals(_person) && _typeOfRule == casted._typeOfRule &&
					casted._period.Equals(_period) && casted._dateOnlyPeriod.Equals(_dateOnlyPeriod));
		}

        public override int GetHashCode()
        {
            return (_person.GetHashCode() ^ _typeOfRule.GetHashCode() ^ _period.GetHashCode() ^ _dateOnlyPeriod.GetHashCode());
        }
    }
}