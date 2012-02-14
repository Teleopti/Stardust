using System;
using System.IdentityModel.Tokens;
using System.Security.Principal;

namespace Teleopti.Ccc.Sdk.Common.WcfExtensions
{
    public class CustomWindowsSecurityToken : WindowsSecurityToken, ITokenWithBusinessUnitAndDataSource
    {
        private readonly string _dataSource;
        private readonly Guid _businessUnit;

        public CustomWindowsSecurityToken(WindowsIdentity windowsIdentity, string dataSource, Guid businessUnit)
            : base(windowsIdentity)
        {
            _dataSource = dataSource;
            _businessUnit = businessUnit;
        }

        public Guid BusinessUnit
        {
            get { return _businessUnit; }
        }

        public bool HasBusinessUnit
        {
            get { return _businessUnit != Guid.Empty; }
        }

        public string DataSource
        {
            get { return _dataSource; }
        }
    }
}