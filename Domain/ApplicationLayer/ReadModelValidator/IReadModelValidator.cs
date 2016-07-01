using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public interface IReadModelValidator
	{
		void Validate(DateTime start,DateTime end,Action<ReadModelValidationResult> reportProgress,
			bool ignoreValid = false);

		void SetTargetTypes(IList<ValidateReadModelType> types);		
	}
}