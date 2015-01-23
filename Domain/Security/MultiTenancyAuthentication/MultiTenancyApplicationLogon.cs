using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class MultiTenancyApplicationLogon : IMultiTenancyApplicationLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		
		public MultiTenancyApplicationLogon(IRepositoryFactory repositoryFactory, IAuthenticationQuerier authenticationQuerier)
		{
			_repositoryFactory = repositoryFactory;
			_authenticationQuerier = authenticationQuerier;
		}

		public AuthenticationResult Logon(ILogonModel logonModel, IApplicationData applicationData)
		{
			var result = _authenticationQuerier.TryLogon(logonModel.UserName, logonModel.Password);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false, 
					HasMessage = true,
					Message = result.FailReason
				};

			var dataSourceName = result.Tennant;
			var personId = result.PersonId;

			// later send the encrypted config from logon to here
			var krypterad =
				@"4gDEvx5C5+Ib30tiCBsvr+UMhdQPRTi7IX9RaZjVWMzN/78k454m3WufD9PCZjYouQAmOT+VNdJYDORaq0LXMPqyB1KZ9jJ3H3PO9ndeX6YdGlZ54qBS6uj1vbUH2c8rO3K3g7Htl1HDEN/vbjyZz1XivaymnDR4g0EOOdgAP5o6AgllqaI9W3L6qMEzx0vhbqQFl2Nb7wDoNE0ZUn+/4VKUzRb+l/iGXXoymaJP6M3FHi8fxWrSZQQOFl2Q4PQ+tFM3/BqSibcmlWe3gD3laDFTfQORm+MLPPClL06DqGwpa8bsZtXhUjoQ2yOaEQfIEp+0Da3wjYoKDM6pmNjJLTWKPrHRl184faQNhIyyA971jMDCj2NlRu7ct8X5BsHKR5eOoh+MdSnxFZA/oOxECNDwavOtQFGYegtt2r5i9wKWVlhaGuotaS63djhVuvanwH1EsvxSEV60PkDAAYUJuWcAdJTg0YpxJGQWhqMV9JwfStq26TdKv+16KSoY8zdLv7pgnhowiFV2F4P+CxNcP2FznttwZWrDu3XCFcOycEBUWsEObB9jzt1JPuzlRNk5oiP4XHh4zcWKR+g2w/AY97EQMICsunxkDeKgfONAxuOJadH0E/imG6hPLzcEg9kZn/LPAAZC4hpPWze5F4s5C2iYIUUaTIuKS7uQUvRyryqwQl987750MGnSO+Sye0e4EYruKRrrcHnmMr8jwd1znqI85f18pWL9a60W7buwzRKdJIz4Rm+IcVE/f8t/hNZp9CD5zJVbueJBGn2dZR2FmHIrfuboRp7mW00MBPoJ5acCDKxEXnxEnQg4C4dOqe+HRZg/l8OZvgDlUBazq1X6/j/YK3NfuJslB2NhlyUoxbS6OYCzPmxOslEzoJzufZBVQVO/SAre+/RUgbHGVqGtx5L6i63JK5SBTffIYDSxsY0LBCi0cIgqrw2ey64OGQEuQfs5K4cL+W2JXRFxxsYlR72rlfhFPqj5SaBFLcWKcsGF4hJtYU+iYgv5995LtMYF
zjynBDpennBSNqkqCiW3EQRWBLLUsTvYDVTukgp553hrec5dBRnZbAJDPZ1C9vxaL41gULCDALIUiNUtN9oLPUGfTzC5QPXZaIFislDqPgIbGc4P4y+juPHoQ1eIe52i71XCAAEzX1iAYDdqrBnu4WMDjLxfvRviOTHjR8c28s+Pe0x7KGTV/FCYj/Tp7F/8r2XYwSrgKHnPNe34C3YvqxnhJs4DEVFHGIeVYlR9u1bH0oKmNowu1WeDdZcOrw7zJxzlOb+TfxtQWogWD+qtSk8wTc05SXeqoQY2W7wCS1VX6PI5MQPYsgB4MKaYMIV/UniN4N/z9q4ALif/1SJwbMqQGhnIrvd4WG2Hr35KaA0MjfYWK1rZp/SOGwBUfE59imt/0nqFueF7HiDAx82P9hXhB+/ebx6FsFmfuF+Yh/ZUdkn+x12EXZ2dCsTFzmufoQG/dtakH5ubNYuW+y5eD3O6CJaSwqg3jRKBWNo89223+lOsx8G0l+Z2Zt8cnTGU8GE2FBnt/qKMZyj+7zxuDF80KUxnoE+NIgM1rg8rju8=";

			logonModel.SelectedDataSourceContainer = getDataSorce(krypterad, applicationData);
			// if null error
			if (logonModel.SelectedDataSourceContainer == null)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = string.Format(Resources.CannotFindDataSourceWithName, dataSourceName)
				};

			using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var person = _repositoryFactory.CreatePersonRepository(uow).LoadOne(personId);
				logonModel.SelectedDataSourceContainer.SetUser(person);
				logonModel.SelectedDataSourceContainer.LogOnName = logonModel.UserName;
			}

			return new AuthenticationResult
			{
				Person = logonModel.SelectedDataSourceContainer.User,
				Successful = true
			};
		}

		IDataSourceContainer getDataSorce(string encryptedNhib, IApplicationData applicationData)
		{
			var nhibConfig = Encryption.DecryptStringFromBase64(encryptedNhib, EncryptionConstants.Image1, EncryptionConstants.Image2);

			var datasource = applicationData.CreateAndAddDataSource(nhibConfig);

			return new DataSourceContainer(datasource,_repositoryFactory,null,AuthenticationTypeOption.Application);
		}

	}
}