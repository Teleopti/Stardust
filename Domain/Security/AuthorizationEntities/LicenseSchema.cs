using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{

    /// <summary>
    /// Represents the default license schema of Raptor and makes the licence data available.
    /// </summary>
    public class LicenseSchema
    {
        private string _enabledLicenseSchemaCode;
        private ReadOnlyCollection<LicenseOption> _licenseOptions = DefinedLicenseDataFactory.CreateDefinedLicenseOptions();

        #region Interface

        #region Singleton for the active schema

        private static LicenseSchema _activeLicenseSchema;

        /// <summary>
        /// Gets the singleton instance of the active license schema.
        /// </summary>
        /// <value>The instance.</value>
        public static LicenseSchema ActiveLicenseSchema
        {
            get
            {
                 if (_activeLicenseSchema == null)
                    _activeLicenseSchema = DefinedLicenseDataFactory.CreateActiveLicenseSchema();
                return _activeLicenseSchema;
            }
            set { _activeLicenseSchema = value; }
        }

        #endregion

        /// <summary>
        /// Gets or sets the enabled licence schema.
        /// </summary>
        /// <value>The enabled licence schema.</value>
        public string EnabledLicenseSchema
        {
            get { return _enabledLicenseSchemaCode; }
            set { _enabledLicenseSchemaCode = value; }
        }

        /// <summary>
        /// Gets or sets the defined license options.
        /// </summary>
        /// <value>The defined license options.</value>
        public ReadOnlyCollection<LicenseOption> LicenseOptions
        {
            get
            {
                return _licenseOptions;
            }
        }

        /// <summary>
        /// Gets the enabled licence option.
        /// </summary>
        /// <value>The enabled licence option paths.</value>
        /// <returns></returns>
        public ReadOnlyCollection<LicenseOption> EnabledLicenseOptions
        {
            get
            {
                IList<LicenseOption> enabledLicenseOptions = new List<LicenseOption>();
                foreach (LicenseOption licenseOption in _licenseOptions)
                {
                    if (licenseOption.Enabled)
                        enabledLicenseOptions.Add(licenseOption);
                }
                return new ReadOnlyCollection<LicenseOption>(enabledLicenseOptions);
            }
        }

        /// <summary>
        /// Gets the enabled license option paths.
        /// </summary>
        /// <value>The enabled license option paths.</value>
        /// <returns></returns>
        public IList<string> EnabledLicenseOptionPaths
        {
            get
            {
                IList<string> enabledLicenceOptionPaths = new List<string>();
                foreach (LicenseOption licenseOption in EnabledLicenseOptions)
                {
                    enabledLicenceOptionPaths.Add(licenseOption.LicenseOptionPath);
                }
                return enabledLicenceOptionPaths;
            }
        }

        /// <summary>
        /// Sets the defined license options enabled according to the input list.
        /// </summary>
        /// <value>The enabled licence options.</value>
        public void ActivateLicense(ILicenseActivator licenseActivator)
        {
            EnabledLicenseSchema = licenseActivator.EnabledLicenseSchemaName;

            // set to false all
            foreach (LicenseOption licenseOption in LicenseOptions)
            {
                licenseOption.Enabled = false;
            }
            foreach (string licenseOptionPath in licenseActivator.EnabledLicenseOptionPaths)
            {
                LicenseOption licenseOption =
                    LicenseOption.FindLicenseOptionByPath(LicenseOptions, licenseOptionPath);
                if (licenseOption != null)
                    licenseOption.Enabled = true;
            }
        }

        #endregion

    }
}
