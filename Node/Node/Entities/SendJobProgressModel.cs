using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Entities
{
	public class SendJobProgressModel : ISendJobProgressModel, IValidatableObject
	{
		public Guid JobId { get; set; }

		public string ProgressDetail { get; set; }

		public DateTime? Created { get; set; }

		public HttpResponseMessage ResponseMessage { get; set; }

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