using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stardust.Manager.Models
{
	public class JobRequestModel : IValidatableObject
	{
		public string Name { get; set; }

		public string Serialized { get; set; }

		public string Type { get; set; }

		public string UserName { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var list = new List<ValidationResult>();

			var pIncome = new[]
			{
				"Name"
			};

			if (string.IsNullOrEmpty(Name))
			{
				list.Add(new ValidationResult("Invalid job name value.", pIncome));
			}

			if (string.IsNullOrEmpty(Type))
			{
				pIncome = new[]
				{
					"Name"
				};

				list.Add(new ValidationResult("Invalid job type value.", pIncome));
			}

			if (string.IsNullOrEmpty(Serialized))
			{
				pIncome = new[]
				{
					"Serialized"
				};

				list.Add(new ValidationResult("Invalid job serialized value.", pIncome));
			}

			if (string.IsNullOrEmpty(UserName))
			{
				pIncome = new[]
				{
					"UserName"
				};

				list.Add(new ValidationResult("Invalid job user name value.", pIncome));
			}

			return list;
		}
	}
}