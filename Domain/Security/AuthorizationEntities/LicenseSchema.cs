using System.Collections.Concurrent;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	/// <summary>
	/// Represents the default license schema of Raptor and makes the licence data available.
	/// </summary>
	public class LicenseSchema
	{
		private readonly LicenseOption[] _licenseOptions = DefinedLicenseDataFactory.CreateDefinedLicenseOptions();

		private static readonly ConcurrentDictionary<string, LicenseSchema> activeLicenseSchemas = new ConcurrentDictionary<string, LicenseSchema>();
		/// <summary>
		/// Gets the singleton instance of the active license schema.
		/// </summary>
		/// <value>The instance.</value>
		public static LicenseSchema GetActiveLicenseSchema(string dataSource)
		{
			LicenseSchema activeLicenseSchema;
			activeLicenseSchemas.TryGetValue(dataSource, out activeLicenseSchema);
			if (activeLicenseSchema != null)
				return activeLicenseSchema;
			var licenseSchema = DefinedLicenseDataFactory.CreateActiveLicenseSchema(dataSource);
			activeLicenseSchemas.TryAdd(dataSource, licenseSchema);
			return licenseSchema;
		}

		public static void SetActiveLicenseSchema(string dataSource, LicenseSchema licenseSchema)
		{
			activeLicenseSchemas.AddOrUpdate(dataSource, licenseSchema, (d, l) => licenseSchema);
		}

		public static void ClearActiveLicenseSchemas(string tenant)
		{
			activeLicenseSchemas.TryRemove(tenant, out var _);
		}
		/// <summary>
		/// Gets or sets the enabled licence schema.
		/// </summary>
		/// <value>The enabled licence schema.</value>
		public string EnabledLicenseSchema { get; set; }

		/// <summary>
		/// Gets or sets the defined license options.
		/// </summary>
		/// <value>The defined license options.</value>
		public LicenseOption[] LicenseOptions => _licenseOptions;

		/// <summary>
		/// Gets the enabled licence option.
		/// </summary>
		/// <value>The enabled licence option paths.</value>
		/// <returns></returns>
		public LicenseOption[] EnabledLicenseOptions
		{
			get
			{
				return _licenseOptions.Where(licenseOption => licenseOption.Enabled).ToArray();
			}
		}

		/// <summary>
		/// Gets the enabled license option paths.
		/// </summary>
		/// <value>The enabled license option paths.</value>
		/// <returns></returns>
		public string[] EnabledLicenseOptionPaths
		{
			get
			{
				return EnabledLicenseOptions.Select(licenseOption => licenseOption.LicenseOptionPath).ToArray();
			}
		}

		/// <summary>
		/// Sets the defined license options enabled according to the input list.
		/// </summary>
		/// <value>The enabled licence options.</value>
		public void ActivateLicense(ILicenseActivator licenseActivator)
		{
			if (licenseActivator == null)
			{
				throw new LicenseMissingException("Cannot find a valid license for the given data source.");
			}
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
	}
}
