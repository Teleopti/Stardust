using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stardust.Manager.Models
{
	public class JobProgressModel : IValidatableObject
	{
		public Guid JobId { get; set; }

		public string ProgressDetail { get; set; }

		public DateTime Created { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var list = new List<ValidationResult>();

			var pIncome = new[]
			{
				"JobId"
			};

			if (JobId == Guid.Empty)
			{
				list.Add(new ValidationResult("Invalid job id value.", pIncome));
			}

			if (string.IsNullOrEmpty(ProgressDetail))
			{
				pIncome = new[]
				{
					"ProgressDetail"
				};

				list.Add(new ValidationResult("Invalid progress detail value.", pIncome));
			}

			return list;
		}
	}
}