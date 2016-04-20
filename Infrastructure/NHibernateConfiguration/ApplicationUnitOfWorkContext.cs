using System.Threading;
using System.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class ApplicationUnitOfWorkContext
	{
		private static readonly ThreadLocal<IUnitOfWork> _unitOfWork = new ThreadLocal<IUnitOfWork>();
		private const string itemsKey = "ApplicationUnitOfWork";

		public void Set(IUnitOfWork unitOfWork)
		{
			if (HttpContext.Current != null)
			{
				HttpContext.Current.Items[itemsKey] = unitOfWork;
				return;
			}
			_unitOfWork.Value = unitOfWork;
		}

		public IUnitOfWork Get()
		{
			if (HttpContext.Current != null)
				return (IUnitOfWork)HttpContext.Current.Items[itemsKey];
			return _unitOfWork.Value;
		}

		public void Clear()
		{
			Set(null);
		}
	}
}