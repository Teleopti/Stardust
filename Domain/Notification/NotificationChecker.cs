using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationChecker : INotificationChecker
	{
		private readonly ICurrentUnitOfWork _unitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private SmsSettings _setting;

		public NotificationChecker(ICurrentUnitOfWork unitOfWorkFactory, IRepositoryFactory repositoryFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
		}

		private SmsSettings notificationSetting()
		{
			if (_setting != null)
				return _setting;
			var uow = _unitOfWorkFactory.Current();
			return _setting = _repositoryFactory.CreateGlobalSettingDataRepository(uow)
				.FindValueByKey("SmsSettings", new SmsSettings());
		}

		public NotificationType NotificationType()
		{
			return notificationSetting().NotificationSelection;
		}

		public string EmailSender
		{
			get
			{
				return notificationSetting().EmailFrom;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
	        MessageId = "0")]
	    public string SmsMobileNumber(IPerson person)
	    {
	        // get wich optional column to use

			if (notificationSetting().OptionalColumnId.Equals(Guid.Empty)) // no column set
	            return "";
	        // get a value if one
	        foreach (
	            var optionalColumnValue in
	                person.OptionalColumnValueCollection.Where(
						optionalColumnValue => optionalColumnValue.Parent.Id.Equals(notificationSetting().OptionalColumnId)))
	        {
	            return optionalColumnValue.Description;
	        }

	        //no value
	        return "";
	    }
	}
}