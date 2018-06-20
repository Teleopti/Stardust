using System;
using System.Security.Cryptography;
using System.Text;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.Security
{
	public class SignatureCreator
	{
		private readonly Lazy<RSACryptoServiceProvider> provider;

		public SignatureCreator(IConfigReader configReader)
		{
			provider = new Lazy<RSACryptoServiceProvider>(() =>
			{
				var ret = new RSACryptoServiceProvider();
				ret.ImportParameters(new RSAParameters
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
				return ret;
			});
		}

		public string Create(string content)
		{
			return Convert.ToBase64String(provider.Value.SignData(Encoding.UTF8.GetBytes(content), CryptoConfig.MapNameToOID("SHA1")));
		}

		public bool Verify(string content, string signature)
		{
			return provider.Value.VerifyData(Encoding.UTF8.GetBytes(content), CryptoConfig.MapNameToOID("SHA1"), Convert.FromBase64String(signature));
		}
	}
}