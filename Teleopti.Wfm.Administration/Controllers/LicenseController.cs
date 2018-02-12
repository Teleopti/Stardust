using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class LicenseController : ApiController
    {
		private readonly ILicenseRepository _licenseRepository;
		private readonly IPersonRepository _personRepository;

		public LicenseController(ILicenseRepository licenseRepository, IPersonRepository personRepository)
		{
			_licenseRepository = licenseRepository;
			_personRepository = personRepository;
		}
		public async Task<IHttpActionResult> Apply()
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				return StatusCode(HttpStatusCode.UnsupportedMediaType);
			}

			var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();

			foreach (var stream in filesReadToProvider.Contents)
			{
				var fileBytes = await stream.ReadAsByteArrayAsync();
				using (var reader = new StreamReader(new MemoryStream(fileBytes), Encoding.Default))
				{
					var xmlLicenseString = reader.ReadToEnd();
					XDocument signedXml = XDocument.Parse(xmlLicenseString, LoadOptions.None);
					int numberOfActiveAgents = _personRepository.NumberOfActiveAgents();
					var publicKeyXmlContent = new XmlLicensePublicKeyReader().GetXmlPublicKey();
					using (new XmlLicenseService(signedXml, publicKeyXmlContent, numberOfActiveAgents))
					{
						License license = new License { XmlString = xmlLicenseString };

						_licenseRepository.Add(license);
						return Ok();

					}
				}
			}

			return Ok();
		}

	}
}
