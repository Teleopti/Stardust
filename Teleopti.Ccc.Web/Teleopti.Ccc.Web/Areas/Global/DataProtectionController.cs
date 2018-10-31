using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class DataProtectionController : ApiController
	{
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentDataSource _currentDataSource;

		public DataProtectionController(
			IPersonalSettingDataRepository personalSettingDataRepository, 
			ILoggedOnUser loggedOnUser,
			ICurrentDataSource currentDataSource)
		{
			_personalSettingDataRepository = personalSettingDataRepository;
			_loggedOnUser = loggedOnUser;
			_currentDataSource = currentDataSource;
		}

		[UnitOfWork, Route("api/Global/DataProtection/Yes"), HttpPost]
		public virtual IHttpActionResult Yes()
		{
			var setting = _personalSettingDataRepository.FindValueByKey(DataProtectionResponse.Key, new DataProtectionResponse());
			setting.Response = DataProtectionEnum.Yes;
			setting.ResponseDate = DateTime.UtcNow;
			_personalSettingDataRepository.PersistSettingValue(DataProtectionResponse.Key, setting);
			return Ok();
		}

		[UnitOfWork, Route("api/Global/DataProtection/No"), HttpPost]
		public virtual IHttpActionResult No()
		{
			var setting = _personalSettingDataRepository.FindValueByKey(DataProtectionResponse.Key, new DataProtectionResponse());
			setting.Response = DataProtectionEnum.No;
			setting.ResponseDate = DateTime.UtcNow;
			_personalSettingDataRepository.PersistSettingValue(DataProtectionResponse.Key, setting);
			return Ok();
		}

		[UnitOfWork, Route("api/Global/DataProtection/QuestionText"), HttpGet]
		public virtual IHttpActionResult QuestionText()
		{
			var user = _loggedOnUser.CurrentUser();
			var currentDatasourceName = _currentDataSource.CurrentName();
			var customerName = DefinedLicenseDataFactory.GetLicenseActivator(currentDatasourceName).CustomerName;
			return Ok(string.Format(Resources.DataProtectionQuestion, user.Email.Trim(), user.Name.FirstName, user.Name.LastName, user.PermissionInformation.UICulture().DisplayName, customerName));
		}
	}
}