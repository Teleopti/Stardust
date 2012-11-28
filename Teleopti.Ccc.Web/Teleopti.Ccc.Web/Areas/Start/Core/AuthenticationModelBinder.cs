using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Start.Core
{
	public class AuthenticationModelBinder : IModelBinder
	{
		private readonly IEnumerable<IAuthenticationType> _types;

		public AuthenticationModelBinder(IEnumerable<IAuthenticationType> types)
		{
			_types = types;
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var typeString = bindingContext.ValueProvider.GetValue("type").AttemptedValue;
			var type = _types.SingleOrDefault(t => t.TypeString == typeString);
			if (type == null)
				throw new NotImplementedException("Authentication type " + typeString + " not found");
			return type.BindModel(bindingContext);
		}
	}
}