using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stardust.Manager.Models
{
	public class JobDetail : IValidatableObject
	{
		public int Id { get; set; }

		public Guid JobId { get; set; }

		public string Detail { get; set; }

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

			if (!string.IsNullOrEmpty(Detail)) return list;

			pIncome = new[]
			{
				"Detail"
			};

			list.Add(new ValidationResult("Invalid detail value.", pIncome));

			return list;
		}
	}
}