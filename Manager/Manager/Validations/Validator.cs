using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Stardust.Manager.Validations
{
	public class Validator
	{
		private const string JobIdIsInvalid = "Job Id is invalid.";
		private const string NodeUriIsInvalid = "Node Uri is invalid.";

		public ObjectValidationResult ValidateObject(IValidatableObject validatableObject)
		{
			if (validatableObject == null)
			{
				return new ObjectValidationResult
				{
					Message = "Object can not be null."
				};
			}

			var validationResults =
				validatableObject.Validate(new ValidationContext(this));

			var enumerable =
				validationResults as IList<ValidationResult> ?? validationResults.ToList();

			if (enumerable.Any())
			{
				return new ObjectValidationResult
				{
					Message = enumerable.First().ErrorMessage
				};
			}

			return new ObjectValidationResult
			{
				Success = true
			};
		}

		public ObjectValidationResult ValidateJobId(Guid jobId)
		{
			if (jobId == Guid.Empty)
			{
				return new ObjectValidationResult
				{
					Message = JobIdIsInvalid
				};
			}

			return new ObjectValidationResult { Success = true };
		}

		public ObjectValidationResult ValidateUri(Uri uri)
		{
			if (uri == null)
			{
				return new ObjectValidationResult
				{
					Message = NodeUriIsInvalid
				};
			}

			return new ObjectValidationResult{Success = true};
		}
	}
}
