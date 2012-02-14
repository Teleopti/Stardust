using System.Web.Mvc;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Filters
{
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