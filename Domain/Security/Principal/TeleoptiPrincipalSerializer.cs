using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.IdentityModel.Claims;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Xml;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ITeleoptiPrincipalSerializer
	{
		string Serialize(TeleoptiPrincipalSerializable principal);
		TeleoptiPrincipalSerializable Deserialize(string data);
	}

	public class TeleoptiPrincipalGZipBase64Serializer : TeleoptiPrincipalSerializerCore, ITeleoptiPrincipalSerializer
	{
		public TeleoptiPrincipalGZipBase64Serializer(IApplicationData applicationData, IBusinessUnitRepository businessUnitRepository, IPersonRepository personRepository) 
			: base(applicationData, businessUnitRepository, personRepository)
		{
			
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public string Serialize(TeleoptiPrincipalSerializable principal)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var compressingStream = new GZipStream(memoryStream, CompressionMode.Compress))
				{
					var writer = XmlDictionaryWriter.CreateBinaryWriter(compressingStream);
					Serializer.WriteObject(writer, principal);
					writer.Flush();
				}
				var base64 = Convert.ToBase64String(memoryStream.ToArray());
				return base64;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public TeleoptiPrincipalSerializable Deserialize(string data)
		{
			TeleoptiPrincipalSerializable principal;
			var bytes = Convert.FromBase64String(data);
			using (var memoryStream = new MemoryStream(bytes))
			{
				using (var decompressingStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					var reader = XmlDictionaryReader.CreateBinaryReader(decompressingStream, XmlDictionaryReaderQuotas.Max);
					principal = (TeleoptiPrincipalSerializable)Serializer.ReadObject(reader);
				}
			}
			return PopulateData(principal);
		}

	}

	//public class TeleoptiPrincipalXmlFileSerializer : TeleoptiPrincipalSerializerCore, ITeleoptiPrincipalSerializer
	//{
	//    private readonly string _file;

	//    public TeleoptiPrincipalXmlFileSerializer(IApplicationData applicationData, IBusinessUnitRepository businessUnitRepository, string file)
	//        : base(applicationData, businessUnitRepository) {
	//        _file = file;
	//        }

	//    public string Serialize(TeleoptiPrincipalSerializable principal)
	//    {
	//        using (var stream = new FileStream(_file, FileMode.Create))
	//        {
	//            Serializer.WriteObject(stream, principal);
	//        }
	//        return System.IO.File.ReadAllText(_file);
	//    }

	//    public TeleoptiPrincipalSerializable Deserialize(string data)
	//    {
	//        TeleoptiPrincipalSerializable principal;
	//        using (var memoryStream = new MemoryStream())
	//        {
	//            var writer = new StreamWriter(memoryStream);
	//            writer.Write(data);
	//            writer.Flush();
	//            memoryStream.Position = 0;
	//            principal = (TeleoptiPrincipalSerializable)Serializer.ReadObject(memoryStream);
	//        }
	//        return PopulateData(principal);
	//    }
	//}







	public class TeleoptiPrincipalSerializerCore
	{
		//private static readonly DataContractJsonSerializer Serializer = new DataContractJsonSerializer(
		//    typeof(TeleoptiPrincipalSerializable),
		//    "root",
		//    new[]
		//        {
		//            typeof (TeleoptiIdentity),
		//            typeof (DefaultClaimSet),
		//            typeof (Regional),
		//            typeof (CccTimeZoneInfo),
		//            typeof (OrganisationMembership), 
		//            typeof (GregorianCalendar)
		//        },
		//    2048, // just a number...
		//    false,
		//    null,
		//    true
		//    );

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		protected static readonly DataContractSerializer Serializer = new DataContractSerializer(
			typeof (TeleoptiPrincipalSerializable),
			"TeleoptiPrincipal",
			"http://schemas.ccc.teleopti.com/sdk/2012/05/",
			new[]
				{
					typeof (TeleoptiIdentity),
					typeof (DefaultClaimSet),
					typeof (Regional),
					typeof (CccTimeZoneInfo),
					typeof (OrganisationMembership),
					typeof (AuthorizeApplicationFunction),
					typeof (AuthorizeExternalAvailableData),
					typeof (AuthorizeMyOwn),
					typeof (AuthorizeMyTeam),
					typeof (AuthorizeMySite),
					typeof (AuthorizeMyBusinessUnit),
					typeof (AuthorizeEveryone)
				},
			32000, // just a number...
			false,
			true,
			null
			);

		private readonly IApplicationData _applicationData;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IPersonRepository _personRepository;

		public TeleoptiPrincipalSerializerCore(IApplicationData applicationData, IBusinessUnitRepository businessUnitRepository, IPersonRepository personRepository)
		{
			_applicationData = applicationData;
			_businessUnitRepository = businessUnitRepository;
			_personRepository = personRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected TeleoptiPrincipalSerializable PopulateData(TeleoptiPrincipalSerializable principal)
		{
			principal.Person = _personRepository.Load(principal.PersonId);
			var identity = (TeleoptiIdentity)principal.Identity;
			identity.WindowsIdentity = WindowsIdentity.GetCurrent();
			identity.DataSource = GetDataSourceByName(identity.DataSourceName);
			identity.BusinessUnit = _businessUnitRepository.Load(identity.BusinessUnitId);
			return principal;
		}

		private IDataSource GetDataSourceByName(string dataSourceName)
		{
			var dataSources = _applicationData.RegisteredDataSourceCollection ?? new IDataSource[] { };
			return (
					from d in dataSources
					where d.DataSourceName == dataSourceName
					select d)
				.SingleOrDefault();
		}
	}

}