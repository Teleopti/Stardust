using System;
using System.Security.Cryptography;
using System.Text;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.Security
{
	public class SignatureCreator
	{
		private readonly RSACryptoServiceProvider provider;

		public SignatureCreator(IConfigReader configReader)
		{
			provider = new RSACryptoServiceProvider();
			provider.ImportParameters(new RSAParameters
			{
				D = Convert.FromBase64String(configReader.AppConfig("CertificateD")),
				DP = Convert.FromBase64String(configReader.AppConfig("CertificateDP")),
				DQ = Convert.FromBase64String(configReader.AppConfig("CertificateDQ")),
				Exponent = Convert.FromBase64String(configReader.AppConfig("CertificateExponent")),
				InverseQ = Convert.FromBase64String(configReader.AppConfig("CertificateInverseQ")),
				Modulus = Convert.FromBase64String(configReader.AppConfig("CertificateModulus")),
				P = Convert.FromBase64String(configReader.AppConfig("CertificateP")),
				Q = Convert.FromBase64String(configReader.AppConfig("CertificateQ"))
			});
		}

		public string Create(string content)
		{
			return Convert.ToBase64String(provider.SignData(Encoding.UTF8.GetBytes(content), CryptoConfig.MapNameToOID("SHA1")));
		}
	}
}