using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.Win.Sikuli.Validators.RootValidators
{
	internal abstract class RootValidator : IRootValidator
	{
		private IList<IAtomicValidator> _atomicValidators;

		public virtual string Description 
		{
			get
			{
				if (!AtomicValidators.Any())
					return string.Empty;
				var stringBuilder = new StringBuilder();
				foreach (var validator in AtomicValidators)
				{
					stringBuilder.AppendLine(validator.Description);
				}
				return stringBuilder.ToString();
			} 
		}

		public abstract SikuliValidationResult Validate(ITestDuration duration);

		protected IList<IAtomicValidator> AtomicValidators
		{
			get { return _atomicValidators ?? (_atomicValidators = new List<IAtomicValidator>()); }
		}

		protected SikuliValidationResult ValidateAtomicValidators(IEnumerable<IAtomicValidator> validators)
		{
			var result = new SikuliValidationResult();
			foreach (var validator in validators)
			{
				var validatorResult = validator.Validate();
				result.CombineResultValue(validatorResult);
				result.CombineDetails(validatorResult);
			}
			return result;
		}
	}
}
