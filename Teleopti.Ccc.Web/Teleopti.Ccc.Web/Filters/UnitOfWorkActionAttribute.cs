using System;
using System.Linq;
using System.Web.Mvc;
using AutofacContrib.DynamicProxy2;
using Castle.Core.Interceptor;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Filters
{
	//public class UnitOfWorkInterfaceAttribute : InterceptAttribute
	//{
	//    public UnitOfWorkInterfaceAttribute() : base(typeof(UnitOfWorkActionInterceptor)) { }
	//}

	//public class UnitOfWorkActionInterceptor : IInterceptor
	//{
	//    public void Intercept(IInvocation invocation)
	//    {
	//        var attributes = invocation.Method.GetCustomAttributes(typeof (UnitOfWorkActionAttribute), true);
	//        if (attributes.Any())
	//        {
	//            var unitOfWorkFactory = DependencyResolver.Current.GetService<IUnitOfWorkFactory>();
	//            unitOfWorkFactory.CreateAndOpenUnitOfWork();

	//            invocation.Proceed();

	//            var currentUnitOfWork = unitOfWorkFactory.CurrentUnitOfWork();
	//            if (currentUnitOfWork != null)
	//            {
	//                currentUnitOfWork.PersistAll();
	//                currentUnitOfWork.Dispose();
	//            }
	//        }
	//        else
	//        {
	//            invocation.Proceed();
	//        }
	//    }
	//}

	//public class UnitOfWorkActionAttribute : Attribute
	//{
		
	//}

	public class UnitOfWorkActionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			var unitOfWorkFactory = DependencyResolver.Current.GetService<IUnitOfWorkFactory>();
			unitOfWorkFactory.CreateAndOpenUnitOfWork();
		}

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			base.OnResultExecuted(filterContext);

			var unitOfWorkFactory = DependencyResolver.Current.GetService<IUnitOfWorkFactory>();
			var currentUnitOfWork = unitOfWorkFactory.CurrentUnitOfWork();
			if (currentUnitOfWork != null)
			{
				currentUnitOfWork.PersistAll();
				currentUnitOfWork.Dispose();
			}
		}
	}
}