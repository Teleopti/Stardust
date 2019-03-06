using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using Teleopti.Ccc.Domain.Coders;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Client.Http;
using DetailLevel = Teleopti.Interfaces.Domain.DetailLevel;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollExportFeedbackEx : IServiceBusPayrollExportFeedback
	{
		private readonly InterAppDomainArguments _interAppDomainArguments;
		private readonly Guid _payrollResultId;
		public readonly List<PayrollResultDetailData> PayrollResultDetails = new List<PayrollResultDetailData>();
		private readonly HttpClientM _httpClientM;

		public PayrollExportFeedbackEx(InterAppDomainArguments interAppDomainArguments)
		{
			_interAppDomainArguments = interAppDomainArguments;
			_payrollResultId = _interAppDomainArguments.PayrollResultId;
			var httpClient = new HttpClient(
				new HttpClientHandler
				{
					Credentials = CredentialCache.DefaultNetworkCredentials
				});
			var messageBrokerUrl = new MutableUrl();
			var url = ConfigurationManager.AppSettings["MessageBroker"];
			messageBrokerUrl.Configure(url);
			_httpClientM = new HttpClientM(new HttpServer(httpClient,NewtonsoftJsonSerializer.Make()),messageBrokerUrl );
		}

		public virtual void ReportProgress(int percentage, string information)
		{
			var payrollExportProgress = new JobResultProgressLight
			{
				Message = information,
				Percentage = percentage,
				JobResultId = _payrollResultId
			};
			JobResultProgressEncoder payrollResultProgressEncoder = new JobResultProgressEncoder();
			var binaryData =
				payrollResultProgressEncoder.Encode(payrollExportProgress);

			var message = createNotifications(
				_interAppDomainArguments.DataSource,
				Subscription.IdToString(_interAppDomainArguments.BusinessUnitId),
				DateTime.UtcNow,
				DateTime.UtcNow,
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				typeof(IJobResultProgress),
				DomainUpdateType.NotApplicable,
				binaryData,
				Guid.Empty);

			_httpClientM.Post("MessageBroker/NotifyClients", message);
		}

		private Message createNotifications(
			string dataSource,
			string businessUnitId,
			DateTime eventStartDate,
			DateTime eventEndDate,
			Guid moduleId,
			Guid referenceObjectId,
			Guid domainObjectId,
			Type domainObjectType,
			DomainUpdateType updateType,
			byte[] domainObject,
			Guid trackId)
		{

			var eventStartDateString = Subscription.DateToString(eventStartDate);
			var eventEndDateString = Subscription.DateToString(eventEndDate);
			var moduleIdString = Subscription.IdToString(moduleId);
			var domainObjectIdString = Subscription.IdToString(domainObjectId);
			var domainQualifiedTypeString = domainObjectType.AssemblyQualifiedName;
			var domainReferenceIdString = Subscription.IdToString(referenceObjectId);
			var domainObjectString = (domainObject != null) ? Convert.ToBase64String(domainObject) : null;
			var trackIdString = Subscription.IdToString(trackId);
			return new Message
			{
				StartDate = eventStartDateString,
				EndDate = eventEndDateString,
				DomainId = domainObjectIdString,
				DomainType = domainObjectType.Name,
				DomainQualifiedType = domainQualifiedTypeString,
				DomainReferenceId = domainReferenceIdString,
				ModuleId = moduleIdString,
				DomainUpdateType = (int)updateType,
				DataSource = dataSource,
				BusinessUnitId = businessUnitId,
				BinaryData = domainObjectString,
				TrackId = trackIdString
			};

		}

		public void Error(string message)
		{
			Error(message, null);
		}

		public void Error(string message, Exception exception)
		{
			addPayrollResultDetailToList(DetailLevel.Error, message, exception);
		}

		public void Warning(string message)
		{
			Warning(message, null);
		}

		public void Warning(string message, Exception exception)
		{
			addPayrollResultDetailToList(DetailLevel.Warning, message, exception);
		}

		public void Info(string message)
		{
			Info(message, null);
		}

		public void Info(string message, Exception exception)
		{
			addPayrollResultDetailToList(DetailLevel.Info, message, exception);
		}

		public void SetPayrollResult(IPayrollResult payrollResult)
		{
			//throw new NotImplementedException();
		}

		public void AddPayrollResultDetail(IPayrollResultDetail payrollResultDetail)
		{
			Exception exception = null;
			if (!payrollResultDetail.ExceptionMessage.IsNullOrEmpty())
				exception = new Exception(payrollResultDetail.ExceptionMessage);

			PayrollResultDetails.Add(new PayrollResultDetailData(payrollResultDetail.DetailLevel.Convert(), payrollResultDetail.Message, exception, DateTime.UtcNow));
		}

		private void addPayrollResultDetailToList(DetailLevel detailLevel, string message, Exception exception)
		{
			if(exception != null)
			{
				var exceptionMessage = $"Exception Type: {exception.GetType()} Message: {exception.Message} {exception.StackTrace}";
				if (exception.InnerException != null)
				{
					exceptionMessage += $" InnerException Type: {exception.InnerException.GetType()} Message: {exception.InnerException.Message}";
				}
				exception = new Exception(exceptionMessage);
			}
			PayrollResultDetails.Add(new PayrollResultDetailData(detailLevel, message, exception, DateTime.UtcNow));
		}

		public void Dispose()
		{

		}
	}
}