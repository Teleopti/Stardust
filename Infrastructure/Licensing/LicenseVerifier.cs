﻿using System;
using System.Globalization;
using System.Xml;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class LicenseVerifier : ILicenseVerifier
	{
		private readonly ILicenseFeedback _licenseFeedback;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILicenseRepository _licenseRepository;

		public LicenseVerifier(ILicenseFeedback licenseFeedback, IUnitOfWorkFactory unitOfWorkFactory, ILicenseRepository licenseRepository)
		{

			_licenseFeedback = licenseFeedback;
			_unitOfWorkFactory = unitOfWorkFactory;
			_licenseRepository = licenseRepository;
		}

		public ILicenseService LoadAndVerifyLicense()
		{
			using (PerformanceOutput.ForOperation("Verifying license"))
			{
				ILicenseService licenseService;
				try
				{
					licenseService = XmlLicenseService();

					if (licenseService.ExpirationDate.Subtract(licenseService.ExpirationGracePeriod) < DateTime.Now)
					{
						string warningMessage =
							String.Format(CultureInfo.CurrentCulture, Resources.YourProductActivationKeyWillExpireDoNotForgetToRenewItInTime,
										  licenseService.ExpirationDate);
						_licenseFeedback.Warning(warningMessage, Resources.LogOn);
					}
				}
				catch (LicenseMissingException)
				{
					_licenseFeedback.Error(_unitOfWorkFactory.Name + "\r\n" + Resources.NoProductActivationKeyPleaseApplyANewOne);
					licenseService = null;
				}
				catch (SignatureValidationException)
				{
					_licenseFeedback.Error(_unitOfWorkFactory.Name + "\r\n" + Resources.ProductActivationKeyIsInvalidPerhapsForgedPleaseApplyANewOne);
					licenseService = null;
				}
				catch (LicenseExpiredException)
				{
					_licenseFeedback.Error(_unitOfWorkFactory.Name + "\r\n" + Resources.ProductActivationKeyHasExpiredPleaseApplyANewOne);
					licenseService = null;
				}
				catch (TooManyActiveAgentsException e)
				{
					// shouldn't happen now
					string explanation = getTooManyAgentsExplanation(e);
					_licenseFeedback.Error(explanation);
					licenseService = null;
				}
				catch (XmlException)
				{
					_licenseFeedback.Error(Resources.ProductActivationKeyIsInvalidPerhapsForgedPleaseApplyANewOne);
					licenseService = null;
				}
				catch (FormatException)
				{
					_licenseFeedback.Error(Resources.ProductActivationKeyIsInvalidPerhapsForgedPleaseApplyANewOne);
					licenseService = null;
				}
				catch (MajorVersionNotFoundException)
				{
					_licenseFeedback.Error(_unitOfWorkFactory.Name + "\r\n" + Resources.ProductActivationKeyMajorVersionWrong);
					licenseService = null;
				}
				return licenseService;
			}
		}

		private string getTooManyAgentsExplanation(TooManyActiveAgentsException e)
		{
			var datasourceName = _unitOfWorkFactory.Name;
			if (e.LicenseType == LicenseType.Agent)
				return datasourceName + "\r\n" + String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManyActiveAgents,
								 e.NumberOfAttemptedActiveAgents, e.NumberOfLicensed);
			return datasourceName + "\r\n" + String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManySeats,
								 e.NumberOfLicensed);
		}

		protected virtual ILicenseService XmlLicenseService()
		{
			//dont' check the use of agent here now
			return new XmlLicenseServiceFactory().Make(_licenseRepository, 0);
		}
	}
}
