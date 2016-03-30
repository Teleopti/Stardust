using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Stardust.Manager.ActionResults;

namespace Stardust.Manager.Validations
{
	public class Validator
	{
		public IHttpActionResult ValidateObject(IValidatableObject validatableObject,
												 HttpRequestMessage requestMessage)
		{
			if (validatableObject == null)
			{
				return new BadRequestWithReasonPhrase("Object can not be null.");
			}

			var validationResults =
				validatableObject.Validate(new ValidationContext(this));

			var enumerable =
				validationResults as IList<ValidationResult> ?? validationResults.ToList();

			if (enumerable.Any())
			{
				return new BadRequestWithReasonPhrase(enumerable.First().ErrorMessage);
			}

			if (requestMessage == null)
			{
				requestMessage = new HttpRequestMessage();
			}

			return new OkResult(requestMessage);
		}

	}
}
