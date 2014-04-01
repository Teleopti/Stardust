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

		/// <summary>
		/// Gets the XML public key that can be used to verify the license signature.
		/// </summary>
		/// <returns>public key</returns>
		/// <remarks>
		/// Created by: Klas
		/// Created date: 2008-12-03
		/// </remarks>
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
		/// <summary>
		/// Persists a new license in the database.
		/// </summary>
		/// <param name="licenseFilePath">The file path to the new license to be persisted.</param>
		/// <param name="licenseRepository">The license repository to use for saving.</param>
		/// <param name="publicKeyXmlContent">The public key XML used for validating the signature.</param>
		/// <param name="personRepository">The person repository used for validate that the license covers enough active agents.</param>
		/// <remarks>
		/// Created by: Klas
		/// Created date: 2008-12-03
		/// </remarks>
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

		/// <summary>
		/// Persist the new license in the database
		/// </summary>
		/// <param name="licenseFilePath">The file path to the new license to be persisted.</param>
		/// <param name="uow">The uow.</param>
		/// <param name="licenseRepository">The license repository.</param>
		/// <param name="xmlPublicKey">The XML public key.</param>
		/// <param name="personRepository">The person repository.</param>
		/// <exception cref="IOException">when there is a problem reading the license file</exception>
		/// <exception cref="LicenseExpiredException">when the license has expired</exception>
		/// <exception cref="SignatureValidationException">when the license signature is invalid, possibly due to attempted fraud through tampering</exception>
		/// <exception cref="TooManyActiveAgentsException">when there are more active agents in the system than is covered by the license</exception>
		/// <exception cref="XmlException">when the license xml contains an unrecognized tag</exception>
		/// <exception cref="DataSourceException">when there is a problem persisting to the database</exception>
		/// <remarks>
		/// Created by: Klas
		/// Created date: 2008-12-03
		/// Changed:    Henry Greijer 2009-02-14    Had to inject IUnitOfWork, xmlPublicKey and the 2 repositories to achieve better coverage.
		/// </remarks>
		public void SaveNewLicense(string licenseFilePath, IUnitOfWork uow, ILicenseRepository licenseRepository, string xmlPublicKey, IPersonRepository personRepository)
		{
			try
			{
				ILicenseService licenseService = SaveNewLicense(licenseFilePath, licenseRepository, xmlPublicKey, personRepository);
				uow.PersistAll();
				// this is not really needed, but a good thing if we ever want to try to continue without restarting after installing a license
				LicenseProvider.ProvideLicenseActivator(licenseService);
			}
			catch (HibernateException e)
			{
				throw new DataSourceException("Failed to save License: " + e.Message, e);
			}
		}
	}
}