using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators
{
	internal abstract class RootValidator : IRootValidator
	{
		private IList<IAtomicValidator> _atomicValidators;

		public virtual string Name
		{
			get
			{
				var fullClassName = GetType().Name;
				var name = fullClassName;
				if (fullClassName.EndsWith("Validator"))
				{
					name = name.Substring(0, name.Length - "Validator".Length);
				}
				return name;
			}
		}

		public virtual string Description
		{
			get
			{
				if (!AtomicValidators.Any())
					return string.Empty;
				return string.Join(" ", AtomicValidators.Select(validator => validator.Description));
			}
		}

		public virtual bool InstantValidation { get { return false; } }

		public abstract SikuliValidationResult Validate(object data, ITimeZoneGuard timeZoneGuard);

		protected IList<IAtomicValidator> AtomicValidators
		{
			get { return _atomicValidators ?? (_atomicValidators = new List<IAtomicValidator>()); }
		}

		
	}
}
