using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public interface ISmsLinkChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
		string SmsMobileNumber(IPerson person);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class SmsLinkChecker : ISmsLinkChecker
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;

		public SmsLinkChecker(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
		}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
	        MessageId = "0")]
	    public string SmsMobileNumber(IPerson person)
	    {
	        var smsSettingsSetting = _repositoryFactory.CreateGlobalSettingDataRepository(_unitOfWorkFactory.CurrentUnitOfWork())
	                                                   .FindValueByKey("SmsSettings", new SmsSettings());
	        if (smsSettingsSetting.OptionalColumnId.Equals(Guid.Empty)) // no column set
	            return "";
	        // get a value if one
	        foreach (
	            var optionalColumnValue in
	                person.OptionalColumnValueCollection.Where(
	                    optionalColumnValue => optionalColumnValue.Parent.Id.Equals(smsSettingsSetting.OptionalColumnId)))
	        {
	            return optionalColumnValue.Description;
	        }

	        //no value
	        return "";
	    }
	}
}