using System;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers
{
	internal static class SikuliValidatorFactory
	{
		internal static class Scheduler
		{

			private static object createInstanceByReflection(string className)
			{
				var assembly = Assembly.GetExecutingAssembly();

				try
				{
					var type = assembly.GetTypes()
						.First(t => t.Name == className);
					return Activator.CreateInstance(type);
				}
				catch (Exception e)
				{
					throw new SikuliCreateInstanceException(string.Concat(assembly.FullName, ".", className, " not found"), e);
				}
			}

			public static IRootValidator CreateValidator(string validatorName)
			{
				var validatorClassName = validatorName + "Validator";
				return createInstanceByReflection(validatorClassName) as IRootValidator;			
			}
		}
	}
}