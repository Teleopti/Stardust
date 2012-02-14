﻿using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.ClientProxies
{
    public class Proxy : ClientBase<ITeleoptiCccSdkInternal>
    {
        public ICollection<PayrollFormatDto> GetPayrollFormats()
        {
            return Channel.GetPayrollFormats();
        }

        public void InitializePayrollFormats(ICollection<PayrollFormatDto> payrollFormatDtos)
        {
            Channel.InitializePayrollFormats(payrollFormatDtos);
        }

        public ICollection<string> GetHibernateConfigurationInternal()
        {
            return Channel.GetHibernateConfigurationInternal();
        }

        public LicenseVerificationResultDto VerifyLicenseInternal()
        {
            return Channel.VerifyLicenseInternal();
        }

        public void TransferSessionInternal(SessionDataDto sessionDataDto)
        {
#pragma warning disable 612,618
            Channel.TransferSessionInternal(sessionDataDto);
#pragma warning restore 612,618
        }

        public IDictionary<string, string> GetAppSettingsInternal()
        {
            return Channel.GetAppSettingsInternal();
        }

        public string GetPasswordPolicy()
        {
            return Channel.GetPasswordPolicy();
        }

        public void CreateServerPayrollExport(PayrollExportDto payrollExport)
        {
            Channel.CreateServerPayrollExport(payrollExport);
        }

		public CommandResultDto ExecuteCommand(CommandDto commandDto)
		{
			return Channel.ExecuteCommand(commandDto);
		}
    }
}