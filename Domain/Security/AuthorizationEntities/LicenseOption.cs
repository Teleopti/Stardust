using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	public class LicenseOption : Entity
	{
		private string _optionCode;
		private string _schemaCode;
		private readonly List<IApplicationFunction> _enabledApplicationFunctions;

		public static LicenseOption FindLicenseOptionByPath(IEnumerable<LicenseOption> licenseOptions, string licenseOptionPath)
		{
			foreach (LicenseOption licenseOption in licenseOptions)
			{
				if (licenseOption.LicenseOptionPath == licenseOptionPath)
					return licenseOption;
			}

			string licenseOptionNameOnTheFly = licenseOptionPath.Replace("/", " ");
			return new LicenseOption(licenseOptionPath, licenseOptionNameOnTheFly);
		}

		public LicenseOption()
		{
			_enabledApplicationFunctions = new List<IApplicationFunction>();
		}

		public LicenseOption(string licenseOptionPath, string optionName)
			: this()
		{
			LicenseOptionPath = licenseOptionPath;
			_optionCode = ApplicationFunction.GetCode(licenseOptionPath);
			_schemaCode = ApplicationFunction.GetParentPath(licenseOptionPath);
			OptionName = optionName;
		}

		public string LicenseOptionPath { get; }

		public virtual string LicenseOptionCode
		{
			get { return _optionCode; }
			set { _optionCode = value; }
		}

		public virtual string LicenseSchemaCode
		{
			get { return _schemaCode; }
			set { _schemaCode = value; }
		}

		public override string ToString()
		{
			return LicenseSchemaCode + " - " + LicenseOptionCode;
		}

		public virtual IApplicationFunction[] EnabledApplicationFunctions => _enabledApplicationFunctions.ToArray();

		protected void EnableFunctions(params IApplicationFunction[] functionsToEnable)
		{
			_enabledApplicationFunctions.Clear();
			_enabledApplicationFunctions.AddRange(functionsToEnable);
		}

		public virtual void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			EnableFunctions(allApplicationFunctions.ToArray());
		}

		public bool Enabled { get; set; }

		public string OptionName { get; }
	}
}