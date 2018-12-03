using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Common
{

	/// <summary>
	/// Specification for deciding whether an agent is active at the company, meaning that
	/// it does NOT have a overpassed terminal date
	/// </summary>
	public class PersonIsActiveSpecification : Specification<IPerson>
    {
		private readonly DateOnly _dayInQuestion;

		public PersonIsActiveSpecification(DateOnly dayInQuestion)
		{
			_dayInQuestion = dayInQuestion;
		}

		public override bool IsSatisfiedBy(IPerson obj)
        {
            if (obj == null)
                return true;
			if (!obj.TerminalDate.HasValue)
				return true;
			return obj.TerminalDate.Value >= _dayInQuestion;
        }
	}
}
