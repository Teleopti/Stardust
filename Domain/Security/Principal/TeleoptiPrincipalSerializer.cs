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

	public class TeleoptiPrincipalSerializer : ITeleoptiPrincipalSerializer
	{
		private readonly IApplicationData _applicationData;
		private readonly IBusinessUnitRepository _businessUnitRepository;

		private static readonly DataContractSerializer Serializer = new DataContractSerializer(
			typeof(TeleoptiPrincipalSerializable),
			"TeleoptiPrincipal",
			"http://schemas.ccc.teleopti.com/sdk/2012/05/",
			new[]
		        {
		            typeof (TeleoptiIdentity),
		            typeof (DefaultClaimSet),
		            typeof (Regional),
		            typeof (CccTimeZoneInfo),
		            typeof (OrganisationMembership), 
		            typeof (GregorianCalendar)
		        },
			1024, // just a number...
			false,
			true,
			null
			);

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

		public TeleoptiPrincipalSerializer(IApplicationData applicationData, IBusinessUnitRepository businessUnitRepository)
		{
			_applicationData = applicationData;
			_businessUnitRepository = businessUnitRepository;
		}

		public string Serialize(TeleoptiPrincipalSerializable principal)
		{
			//return SerializeAsXmlString(principal);
			return SerializeAsBinaryGZippedBase64(principal);
		}

		private static string SerializeAsXmlString(TeleoptiPrincipalSerializable principal)
		{
			using (var memoryStream = new MemoryStream())
			{
				Serializer.WriteObject(memoryStream, principal);
				memoryStream.Position = 0;
				return new StreamReader(memoryStream).ReadToEnd();
			}
		}

		private static string SerializeAsBinaryGZippedBase64(TeleoptiPrincipalSerializable principal)
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

		public TeleoptiPrincipalSerializable Deserialize(string data)
		{
			//var principal = DeserializeXmlString(data);
			var principal = DeserializeBinaryGZippedBase64(data);
			return PopulateData(principal);
		}

		private static TeleoptiPrincipalSerializable DeserializeBinaryGZippedBase64(string data)
		{
			TeleoptiPrincipalSerializable principal;
			var bytes = Convert.FromBase64String(data);
			using (var memoryStream = new MemoryStream(bytes))
			{
				using (var decompressingStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					var reader = XmlDictionaryReader.CreateBinaryReader(decompressingStream, XmlDictionaryReaderQuotas.Max);
					principal = (TeleoptiPrincipalSerializable) Serializer.ReadObject(reader);
				}
			}
			return principal;
		}

		private static TeleoptiPrincipalSerializable DeserializeXmlString(string data)
		{
			TeleoptiPrincipalSerializable principal;
			using (var memoryStream = new MemoryStream())
			{
				var writer = new StreamWriter(memoryStream);
				writer.Write(data);
				writer.Flush();
				memoryStream.Position = 0;
				principal = (TeleoptiPrincipalSerializable) Serializer.ReadObject(memoryStream);
			}
			return principal;
		}

		private TeleoptiPrincipalSerializable PopulateData(TeleoptiPrincipalSerializable principal)
		{
			var identity = (TeleoptiIdentity) principal.Identity;
			identity.WindowsIdentity = WindowsIdentity.GetCurrent();
			identity.DataSource = GetDataSourceByName(identity.DataSourceName);
			identity.BusinessUnit = GetBusinessUnitById(identity.BusinessUnitId);
			return principal;
		}

		private IBusinessUnit GetBusinessUnitById(Guid id) { return _businessUnitRepository.Get(id); }

		private IDataSource GetDataSourceByName(string dataSourceName)
		{
			var dataSources = _applicationData.RegisteredDataSourceCollection ?? new IDataSource[] {};
			return (
					from d in dataSources
			       	where d.DataSourceName == dataSourceName
			       	select d)
				.SingleOrDefault();
		}
	}
}