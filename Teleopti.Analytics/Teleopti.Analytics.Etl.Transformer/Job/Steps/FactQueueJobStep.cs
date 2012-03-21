using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using log4net;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Exceptions;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactQueueJobStep : JobStepBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (FactQueueJobStep));

        public FactQueueJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "fact_queue";
            JobCategory = JobCategoryType.QueueStatistics;
            IsBusinessUnitIndependent = true;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            int affectedRows = 0;
            int chunkTimeSpan;
            //Agg data is never needed in Memory. Hardcode bigger chunks!
            chunkTimeSpan = 30;
            //if (!int.TryParse(ConfigurationManager.AppSettings["chunkTimeSpan"], out chunkTimeSpan))
            //{
            //    chunkTimeSpan = 2;
            //}

            for (DateTime startDateTime = JobCategoryDatePeriod.StartDateUtc;
                startDateTime.AddDays(chunkTimeSpan) < JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1).AddDays(chunkTimeSpan);
                    startDateTime = startDateTime.AddDays(chunkTimeSpan))
            {

                DateTime endDateTime;

                if (startDateTime.AddDays(chunkTimeSpan) < JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1))
                {
                    endDateTime = startDateTime.AddDays(chunkTimeSpan).AddMilliseconds(-1);
                }
                else
                {
                    endDateTime = JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1);
                }

                var period = new DateTimePeriod(startDateTime, endDateTime);

                affectedRows += _jobParameters.Helper.Repository.FillFactQueueDataMart(period, _jobParameters.DataSource, _jobParameters.DefaultTimeZone);
                Result.RowsAffected = affectedRows;
            }
            //Send to MessageBroker that new queue stats is loaded
            var messageSender = _jobParameters.Helper.MessageSender;
            try
            {
                if(!messageSender.IsAlive)
                    messageSender.InstantiateBrokerService();
                if (messageSender.IsAlive)
                {
                	var identity = (TeleoptiIdentity) TeleoptiPrincipal.Current.Identity;
                    messageSender.SendData(JobCategoryDatePeriod.StartDateUtcFloor,
                                           JobCategoryDatePeriod.EndDateUtcCeiling,
                                           Guid.NewGuid(),
                                           Guid.Empty,
                                           typeof (IStatisticTask),
                                           DomainUpdateType.Insert,
										   identity.DataSource.DataSourceName,
										   identity.BusinessUnit.Id.GetValueOrDefault());
                }
            }
            catch (BrokerNotInstantiatedException exception)
            {
                Logger.Error("An error occured while trying to notify clients via Message Broker.",exception);
            }
            catch (SocketException socketException)
            {
                Logger.Error("An error occured while trying to notify clients via Message Broker.", socketException);
            }
            return affectedRows;
        }
    }
}