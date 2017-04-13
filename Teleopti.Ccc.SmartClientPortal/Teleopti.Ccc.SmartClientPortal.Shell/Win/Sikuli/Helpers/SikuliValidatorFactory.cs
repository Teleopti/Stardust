using System;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.Win.Sikuli.Helpers
{
	internal static class SikuliValidatorFactory
	{
		internal static class Scheduler
		{

			private static object createInstanceByReflection(string className)
			{
				var assembly = Assembly.GetExecutingAssembly();

				var type = assembly.GetTypes()
					.First(t => t.Name == className);

				return Activator.CreateInstance(type);
			}

			public static IRootValidator CreateValidator(string validatorName)
			{
				var validatorClassName = validatorName + "Validator";
				return createInstanceByReflection(validatorClassName) as IRootValidator;			
			}
		}
	}
}