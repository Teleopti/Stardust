using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stardust.Node.Entities
{
	public class JobFailedModel : IValidatableObject
	{
		public Guid JobId { get; set; }

		public AggregateException AggregateException { get; set; }

		public DateTime? Created { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			List<ValidationResult> list = new List<ValidationResult>();

			if (this.JobId == Guid.Empty)
			{
				var pIncome = new[]
				{
					"JobId"
				};

				list.Add(new ValidationResult("Invalid job id value.", pIncome));
			}

			return list;
		}
	}
}