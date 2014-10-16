using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Teleopti.Ccc.WebTest.Filters
{
	public class CustomHttpActionDescriptorForTest : HttpActionDescriptor
	{
		public CustomHttpActionDescriptorForTest(HttpControllerDescriptor httpControllerDescriptor) : base(httpControllerDescriptor)
		{
		}

		public override Collection<HttpParameterDescriptor> GetParameters()
		{
			return new Collection<HttpParameterDescriptor>();
		}

		public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments, CancellationToken cancellationToken)
		{
			return new Task<object>(()=> new object());
		}

		public override string ActionName
		{
			get { return "fake"; }
		}

		public override Type ReturnType
		{
			get { return typeof (string); }
		}
	}
}