using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Xml;
using System.Xml.XPath;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
	[Serializable]
	public class AppdomainCreatorWrapper
	{
		private string _tenantName;
		private string _payrollBasePath;
		private static readonly ILog logger = LogManager.GetLogger(typeof(AppdomainCreatorWrapper));

		public XmlDocument RunPayroll(ISdkServiceFactory sdkServiceFactory, PayrollExportDto payrollExportDto,
			RunPayrollExportEvent @event, Guid payrollResultId,
			IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback, string payrollBasePath)
		{
			var tenantName = payrollExportDto.Name;
			var appDomainArguements = new InterAppDomainArguments
			{
				PayrollExportDto = JsonConvert.SerializeObject(payrollExportDto),
				BusinessUnitId = @event.LogOnBusinessUnitId,
				DataSource = @event.LogOnDatasource,
				UserName = SystemUser.Id.ToString(),
				SdkServiceFactory = sdkServiceFactory,
				PayrollResultId = payrollResultId
			};

			_payrollBasePath = payrollBasePath;

			var appDomain = createAppdomain(tenantName);
			appDomain.SetData(InterAppDomainParameters.AppDomainArgumentsParameter, appDomainArguements);
			appDomain.DoCallBack(runPayroll);
			
			var payrollResultString = appDomain.GetData(InterAppDomainParameters.PayrollResultParameter) as string;

			if (appDomain.GetData(InterAppDomainParameters.PayrollResultDetailsParameter) is List<PayrollResultDetailData> payrollDetailsData)
			{ 
				foreach (var payrollDetailData in payrollDetailsData)
				{
					serviceBusPayrollExportFeedback.AddPayrollResultDetail(new PayrollResultDetail(payrollDetailData.DetailLevel,
						payrollDetailData.Message, payrollDetailData.TimeStamp, payrollDetailData.Exception));
				}
			}

			AppDomain.Unload(appDomain);

			var result = new XmlDocument();
			if (payrollResultString != null)
				result.LoadXml(payrollResultString);
			return result;
		}


		private AppDomain createAppdomain(string appdomainName)
		{
			_tenantName = appdomainName;
			var appdomainSetup =
				new AppDomainSetup
				{
					ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
					ShadowCopyFiles = "true",
					ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
				};

			var trustedLoadFromRemoteSourceGrantSet = new PermissionSet(PermissionState.Unrestricted);//this is the fix
			var appDomain = AppDomain.CreateDomain(appdomainName, null, appdomainSetup, trustedLoadFromRemoteSourceGrantSet);
			appDomain.UnhandledException += AppDomainOnUnhandledException;

			return appDomain;
		}

		private void AppDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
			throw (Exception)(unhandledExceptionEventArgs.ExceptionObject);
		}

		private void runPayroll()
		{
			var appDomainArguments = AppDomain.CurrentDomain.GetData(InterAppDomainParameters.AppDomainArgumentsParameter) as InterAppDomainArguments;
			fixAuthenticationMessageHeader(appDomainArguments);
			var payrollExportDto = JsonConvert.DeserializeObject<PayrollExportDto>(appDomainArguments.PayrollExportDto);
			var sdkServiceFactory = appDomainArguments.SdkServiceFactory;

			IList<IPayrollExportProcessor> processors = new List<IPayrollExportProcessor>();

			var feedback = sdkServiceFactory.CreatePayrollExportFeedback(appDomainArguments);
			
			processors = load(_payrollBasePath, feedback as PayrollExportFeedbackEx);
			
			if (!processors.Any())
			{
				var message = "Unable to run payroll. No payroll export processor found.";
				logger.Error(message);
				feedback.Error(message);
				SetFeedbackData(feedback);
				return;
			}
			
			var processorForCurrentPayroll = processors.FirstOrDefault(p => p.PayrollFormat.FormatId == payrollExportDto.PayrollFormat.FormatId);
			if (processorForCurrentPayroll is IPayrollExportProcessorWithFeedback payrollExportProcessorWithFeedback)
			{
				logger.Debug("The selected payroll export processor can report progress and other feedback.");
				payrollExportProcessorWithFeedback.PayrollExportFeedback = feedback;
			}

			if (processorForCurrentPayroll == null)
			{
				var tenantSpecificPath = Path.Combine(_payrollBasePath, _tenantName);
				var message = $"Payroll export processor for format id: {payrollExportDto.PayrollFormat.FormatId} not found. " +
					$"Please make sure that all payroll files are located for tenant {_tenantName} is located in: {tenantSpecificPath}.";
				logger.Error(message);
				feedback.Error(message);
				SetFeedbackData(feedback);
				return;
			}
		
			// Changing the Base directory of the Appdomain to find non-assembly files used from Payroll processors
			var payrollPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Payroll");
			if (Directory.Exists(Path.Combine(payrollPath, _tenantName)))
				payrollPath = Path.Combine(payrollPath, _tenantName);

			AppDomain.CurrentDomain.SetData("APPBASE", payrollPath);

			IXPathNavigable result;
			var domainAssemblyResolver = new DomainAssemblyResolver(new AssemblyFileLoaderTenant());
			AppDomain.CurrentDomain.AssemblyResolve += domainAssemblyResolver.Resolve;
			try
			{
				result = processorForCurrentPayroll.ProcessPayrollData(sdkServiceFactory.CreateTeleoptiSchedulingService(),
					sdkServiceFactory.CreateTeleoptiOrganizationService(), payrollExportDto);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				feedback.Error("An error occurred while running the payroll export.", ex);
				SetFeedbackData(feedback);
				return;
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= domainAssemblyResolver.Resolve;
			}

			SetFeedbackData(feedback);

			var innerXml = "";
			if(result != null)
				innerXml = result.CreateNavigator()?.InnerXml;

			AppDomain.CurrentDomain.SetData(InterAppDomainParameters.PayrollResultParameter, innerXml);

		}

		private void SetFeedbackData(IPayrollExportFeedback feedback)
		{
			if (feedback is PayrollExportFeedbackEx ex)
				AppDomain.CurrentDomain.SetData(InterAppDomainParameters.PayrollResultDetailsParameter, ex.PayrollResultDetails);
		}

		private static void fixAuthenticationMessageHeader(InterAppDomainArguments appDomainArguments)
		{
			AuthenticationMessageHeader.BusinessUnit = appDomainArguments.BusinessUnitId;
			AuthenticationMessageHeader.DataSource = appDomainArguments.DataSource;
			AuthenticationMessageHeader.UserName = appDomainArguments.UserName;
			AuthenticationMessageHeader.Password = "custom";
			AuthenticationMessageHeader.UseWindowsIdentity = false;
		}

		private IList<IPayrollExportProcessor> load(string path, PayrollExportFeedbackEx feedback)
		{
			var availablePayrollExportProcessors = new List<IPayrollExportProcessor>();
			var domainAssemblyResolver = new DomainAssemblyResolver(new AssemblyFileLoaderTenant());
			{
				AppDomain.CurrentDomain.AssemblyResolve += domainAssemblyResolver.Resolve;
				var tenantSpecificPath = Path.Combine(path, _tenantName);
				try
				{
					var	files = Directory.GetFiles(
						Directory.Exists(tenantSpecificPath) ? tenantSpecificPath : path, "*.dll", SearchOption.TopDirectoryOnly);
				
					foreach (var file in files)
					{
						Assembly.Load(AssemblyName.GetAssemblyName(file));
					}
					foreach (var file in files)
					{
						var assembly = Assembly.Load(AssemblyName.GetAssemblyName(file));
						foreach (var type in assembly.GetExportedTypes())
						{
							if (!type.IsClass || type.IsNotPublic) continue;
							var interfaces = type.GetInterfaces();
							if (!interfaces.Contains(typeof(IPayrollExportProcessor))) continue;
							var obj = Activator.CreateInstance(type);
							var t = (IPayrollExportProcessor)obj;
							availablePayrollExportProcessors.Add(t);
						}
					}
				}
				catch (DirectoryNotFoundException ex)
				{
					var message = $"No payroll is configured for {_tenantName}. Directory not found: {tenantSpecificPath}";
					logger.Error(message);
					feedback?.Error(message, ex);
					return new List<IPayrollExportProcessor>();
				}
				catch (Exception ex)
				{
					var message = $"Problems when loading Payroll files from path: {tenantSpecificPath}  {ex.Message}";
					logger.Error(message);
					feedback?.Error(message, ex);
					return new List<IPayrollExportProcessor>();
				}
				finally
				{ 
					AppDomain.CurrentDomain.AssemblyResolve -= domainAssemblyResolver.Resolve;
				}
			}
			return availablePayrollExportProcessors;
		}

		
		public IList<PayrollFormatDto> FindPayrollFormatsForTenant(string tenantName, string tenantPath)
		{
			_payrollBasePath = tenantPath;
			var appDomain = createAppdomain(tenantName);
				appDomain.DoCallBack(loadAssemblyInternal);
				var payrollFormatDtos = appDomain.GetData(InterAppDomainParameters.PayrollFormatDtosParameter) as IList<PayrollFormatDto>;
			AppDomain.Unload(appDomain);
			return payrollFormatDtos;
		}

		private void loadAssemblyInternal()
		{
			var payrollFormatDtos = new List<PayrollFormatDto>();
			var processors = load(_payrollBasePath, null);

			foreach (var processor in processors)
			{
				payrollFormatDtos.Add(new PayrollFormatDto(processor.PayrollFormat.FormatId, processor.PayrollFormat.Name));
			}
			AppDomain.CurrentDomain.SetData(InterAppDomainParameters.PayrollFormatDtosParameter, payrollFormatDtos);
		}

	}
}
