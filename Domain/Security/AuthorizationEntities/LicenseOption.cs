﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// Represents a licence option that has a collection of enabled functions.
    /// </summary>
    public class LicenseOption : Entity
    {
	    private string _optionCode;
        private string _schemaCode;
	    private readonly List<IApplicationFunction> _enabledApplicationFunctions;

	    /// <summary>
        /// Finds the license option by path.
        /// </summary>
        /// <param name="licenseOptions">The license options.</param>
        /// <param name="licenseOptionPath">The license option path.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseOption"/> class.
        /// </summary>
        public LicenseOption()
        {
            _enabledApplicationFunctions = new List<IApplicationFunction>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseOption"/> class.
        /// </summary>
        /// <param name="licenseOptionPath">The license option path.</param>
        /// <param name="optionName">Name of the option.</param>
        public LicenseOption(string licenseOptionPath, string optionName)
            : this()
        {
            LicenseOptionPath = licenseOptionPath;
            _optionCode = ApplicationFunction.GetCode(licenseOptionPath);
            _schemaCode = ApplicationFunction.GetParentPath(licenseOptionPath);
            OptionName = optionName;
        }

        /// <summary>
        /// Gets or sets the licence option path.
        /// </summary>
        /// <value>The license option path.</value>
        public string LicenseOptionPath { get; }

	    /// <summary>
        /// Gets or sets the code of the license option.
        /// </summary>
        /// <value>The name of the option.</value>
        public virtual string LicenseOptionCode
        {
            get { return _optionCode; }
            set { _optionCode = value; }
        }

        /// <summary>
        /// Gets or sets the code of the license schema.
        /// </summary>
        /// <value>The name of the schema.</value>
        public virtual string LicenseSchemaCode
        {
            get { return _schemaCode; }
            set { _schemaCode = value; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return LicenseSchemaCode + " - " + LicenseOptionCode; 
        }

        /// <summary>
        /// Gets or sets the enabled application functions.
        /// </summary>
        /// <value>The enabled application functions.</value>
        public virtual IApplicationFunction[] EnabledApplicationFunctions => _enabledApplicationFunctions.ToArray();

	    protected void EnableFunctions(params IApplicationFunction[] functionsToEnable)
	    {
		    _enabledApplicationFunctions.Clear();
			_enabledApplicationFunctions.AddRange(functionsToEnable);
	    }

	    /// <summary>
	    /// Sets the enabled (licensed) application functions.
	    /// </summary>
	    /// <param name="allApplicationFunctions">All application functions.</param>
	    /// <value>The enabled application functions.</value>
	    public virtual void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
        {
			EnableFunctions(allApplicationFunctions.ToArray());
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LicenseOption"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

	    public string OptionName { get; }
    }
}
