using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelValidator
	{
		void Validate(ValidateReadModelType types,DateTime start,DateTime end, bool directFix = false);
		void ClearResult(ValidateReadModelType types);
	}
}