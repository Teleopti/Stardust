using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public enum ReadModelValidationMode
	{
		Validate,
		ValidateAndFix,
		Reinitialize
	}


	public interface IReadModelValidator
	{
		void Validate(ValidateReadModelType types,DateTime start,DateTime end, ReadModelValidationMode mode = ReadModelValidationMode.Validate);
		void ClearResult(ValidateReadModelType types);
	}
}