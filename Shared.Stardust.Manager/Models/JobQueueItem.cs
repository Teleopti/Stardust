using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stardust.Manager.Models
{
	public class JobQueueItem : IValidatableObject
	{
		public JobQueueItem()
		{
			Policy = "";
		}

		public Guid JobId { get; set; }

		public string Name { get; set; }

		public string Serialized { get; set; }

		public string Type { get; set; }

		public string CreatedBy { get; set; }

		public DateTime Created { get; set; }

		public string Policy { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var list = new List<ValidationResult>();

			var pIncome = new[]
			{
				"Id"
			};

			if (JobId == Guid.Empty)
			{
				list.Add(new ValidationResult("Invalid parameter: Id", pIncome));
			}

			if (string.IsNullOrEmpty(Name))
			{
				pIncome = new[]
				{
					"Name"
				};

				list.Add(new ValidationResult("Invalid parameter: Name", pIncome));
			}

			if (string.IsNullOrEmpty(Type))
			{
				pIncome = new[]
				{
					"Type"
				};

				list.Add(new ValidationResult("Invalid parameter: Type", pIncome));
			}

			if (string.IsNullOrEmpty(Serialized))
			{
				pIncome = new[]
				{
					"Serialized"
				};

				list.Add(new ValidationResult("Invalid parameter: Serialized", pIncome));
			}

			if (!string.IsNullOrEmpty(CreatedBy)) return list;

			pIncome = new[]
			{
				"CreatedBy"
			};

			list.Add(new ValidationResult("Invalid parameter: CreatedBy", pIncome));


			return list;
		}
	}
}