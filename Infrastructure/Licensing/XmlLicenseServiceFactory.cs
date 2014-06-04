using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using NHibernate;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class XmlLicenseServiceFactory
	{
		public XmlLicenseService Make(ILicenseRepository licenseRepository, int numberOfActiveAgents)
		{
			IList<ILicense> allLicenses = licenseRepository.LoadAll();
			if (allLicenses.Count == 0)
			{
				throw new LicenseMissingException();
			}

			XDocument licenseDocument = XDocument.Parse(allLicenses[0].XmlString);

			string xmlKey = new XmlLicensePublicKeyReader().GetXmlPublicKey();

			return new XmlLicenseService(licenseDocument, xmlKey, numberOfActiveAgents);
		}

		public ILicenseService Make(IUnitOfWorkFactory unitOfWorkFactory, ILicenseRepository licenseRepository, IPersonRepository personRepository)
		{
			using (unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				int numberOfActiveAgents = personRepository.NumberOfActiveAgents();
				return Make(licenseRepository, numberOfActiveAgents);
			}
		}

	}

	public class XmlLicensePublicKeyReader
	{
		private const string publicKeyResourceNameExtention = ".PubKey.xml";

		public string GetXmlPublicKey()
		{
			Stream stream =
					typeof(XmlLicensePublicKeyReader).Assembly.GetManifestResourceStream(typeof(XmlLicensePublicKeyReader).Namespace +
																																 publicKeyResourceNameExtention);

			// Read-in the XML content.
			StreamReader reader = new StreamReader(stream);
			string xmlKey = reader.ReadToEnd();
			reader.Close();
			return xmlKey;
		}

	}

	public class XmlLicensePersister
	{
		public ILicenseService SaveNewLicense(string licenseFilePath, ILicenseRepository licenseRepository,
																								 string publicKeyXmlContent, IPersonRepository personRepository)
		{
			string xmlLicenseString;
			using (var reader = new StreamReader(licenseFilePath))
			{
				xmlLicenseString = reader.ReadToEnd();
			}
			// check if valid
			XDocument signedXml = XDocument.Parse(xmlLicenseString, LoadOptions.None);
			int numberOfActiveAgents = personRepository.NumberOfActiveAgents();
			using (var licenseService = new XmlLicenseService(signedXml, publicKeyXmlContent, numberOfActiveAgents))
			{
				License license = new License { XmlString = xmlLicenseString };

				licenseRepository.Add(license);
				return licenseService;

			}
		}

		public void SaveNewLicense(string licenseFilePath, IUnitOfWorkFactory uowFactory, ILicenseRepository licenseRepository, string xmlPublicKey, IPersonRepository personRepository)
		{
			try
			{
				ILicenseService licenseService = SaveNewLicense(licenseFilePath, licenseRepository, xmlPublicKey, personRepository);
				uowFactory.CurrentUnitOfWork().PersistAll();
				// this is not really needed, but a good thing if we ever want to try to continue without restarting after installing a license
				LicenseProvider.ProvideLicenseActivator(uowFactory.Name, licenseService);
			}
			catch (HibernateException e)
			{
				throw new DataSourceException("Failed to save License: " + e.Message, e);
			}
		}
	}
}