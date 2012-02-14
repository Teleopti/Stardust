using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Teleopti.Ccc.Infrastructure.Licensing.Agreements
{
	public class AgreementProvider
	{
		public static string DefaultAgreement { get { return "Teleopti.Ccc.Infrastructure.Licensing.Agreements.TeleoptiLic_En_Sw_St.txt"; } }
		public static string FreemiumAgreement { get { return "Teleopti.Ccc.Infrastructure.Licensing.Agreements.TeleoptiLic_En_Sw_Forecasts.txt"; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public IList<LicenseAgreement> GetAllAgreements()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			var agreements = new List<LicenseAgreement>();
			foreach (string resourceName in executingAssembly.GetManifestResourceNames())
			{

				if (resourceName.Contains("TeleoptiLic"))
				{
					var agreement = new LicenseAgreement
					{
						ResourceName = resourceName,
						DisplayName = resourceName.Replace("Teleopti.Ccc.Infrastructure.Licensing.Agreements.", ""),
						Agreement = GetFromResources(resourceName)
					};

					if (resourceName.Equals(DefaultAgreement))
					{
						agreements.Insert(0, agreement);
					}
					else
					{
						agreements.Add(agreement);
					}

				}

			}

			return agreements;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public string GetFromResources(string resourceName)
		{
			Assembly assem = GetType().Assembly;
			using (Stream stream = assem.GetManifestResourceStream(resourceName))
			{
				if (stream == null) return "";
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}