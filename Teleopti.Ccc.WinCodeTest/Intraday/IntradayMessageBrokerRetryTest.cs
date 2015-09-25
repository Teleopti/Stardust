using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class IntradayMessageBrokerRetryTest : IDisposable
    {
        private MockRepository mocks;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IIntradayView view;
        private IntradayPresenter target;
        private ISchedulingResultLoader schedulingResultLoader;
        private OnEventStatisticMessageCommand statisticCommand;
        private OnEventScheduleMessageCommand scheduleCommand;
        private OnEventForecastDataMessageCommand forecastCommand;
        private OnEventMeetingMessageCommand meetingCommand;
		private IToggleManager toggleManger;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            view = mocks.StrictMock<IIntradayView>();
			schedulingResultLoader = new SchedulingResultLoader(new SchedulerStateHolder(null, new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), TimeZoneHelper.CurrentSessionTimeZone), new IPerson[] { }, mocks.DynamicMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder(), new TimeZoneGuardWrapper()), null,
                                                     null, null, null,null,null,null,null);
            
            statisticCommand = mocks.StrictMock<OnEventStatisticMessageCommand>();
			forecastCommand = mocks.StrictMock < OnEventForecastDataMessageCommand>();
			scheduleCommand = mocks.StrictMock < OnEventScheduleMessageCommand>();
			meetingCommand = mocks.StrictMock < OnEventMeetingMessageCommand>();
			toggleManger = mocks.StrictMock<IToggleManager>();
            target = new IntradayPresenter(view, schedulingResultLoader, null, null, null, null, unitOfWorkFactory, null,
                                           null,statisticCommand,forecastCommand,scheduleCommand, meetingCommand, null, new Poller(), toggleManger);
        }

        [Test]
        public void ShouldRetryForStatisticEvent()
        {
        	var message = new EventMessage(Guid.NewGuid(),
        	                               DateTime.UtcNow,
        	                               DateTime.UtcNow,
        	                               Guid.NewGuid(), 
        	                               Guid.NewGuid(),
										   Guid.NewGuid(),
        	                               typeof (IStatisticTask).Name,
        	                               DomainUpdateType.Insert,
        	                               string.Empty,
        	                               DateTime.UtcNow);
            using (mocks.Record())
            {
                Expect.Call(view.InvokeRequired).Return(false);
                Expect.Call(()=>statisticCommand.Execute(message)).Throw(new DataSourceException());
                Expect.Call(view.ShowBackgroundDataSourceError);

				Expect.Call(() => statisticCommand.Execute(message));
                Expect.Call(view.HideBackgroundDataSourceError);
            }
            using (mocks.Playback())
            {
                target.OnEventStatisticMessageHandler(null,
                                                      new EventMessageArgs(message));
                target.RetryHandlingMessages();
            }
        }

        [Test]
        public void ShouldHandleDataSourceExceptionInRetry()
        {
        	var message = new EventMessage(Guid.NewGuid(),
        	                               DateTime.UtcNow,
        	                               DateTime.UtcNow, 
        	                               Guid.NewGuid(), 
        	                               Guid.NewGuid(), 
        	                               Guid.NewGuid(),
        	                               typeof (IStatisticTask).Name,
        	                               DomainUpdateType.Insert,
        	                               string.Empty,
        	                               DateTime.UtcNow);
            using (mocks.Record())
            {
                Expect.Call(view.InvokeRequired).Return(false);
				Expect.Call(() => statisticCommand.Execute(message)).Throw(new DataSourceException());
                Expect.Call(view.ShowBackgroundDataSourceError);

				Expect.Call(() => statisticCommand.Execute(message)).Throw(new DataSourceException());
                Expect.Call(view.ShowBackgroundDataSourceError);
            }
            using (mocks.Playback())
            {
                target.OnEventStatisticMessageHandler(null,
                                                      new EventMessageArgs(message));
                target.RetryHandlingMessages();
            }
        }

        [Test,ExpectedException(typeof(InvalidOperationException))]
        public void ShouldHandleOtherExceptionInRetry()
        {
        	var message = new EventMessage(Guid.NewGuid(),
        	                               DateTime.UtcNow,
        	                               DateTime.UtcNow, 
        	                               Guid.NewGuid(),
        	                               Guid.NewGuid(), 
        	                               Guid.NewGuid(),
        	                               typeof (IStatisticTask).Name,
        	                               DomainUpdateType.Insert,
        	                               string.Empty,
        	                               DateTime.UtcNow);
            using (mocks.Record())
            {
                Expect.Call(view.InvokeRequired).Return(false);
				Expect.Call(() => statisticCommand.Execute(message)).Throw(new DataSourceException());
                Expect.Call(view.ShowBackgroundDataSourceError);

				Expect.Call(() => statisticCommand.Execute(message)).Throw(new InvalidOperationException());
            }
            using (mocks.Playback())
            {
                target.OnEventStatisticMessageHandler(null, new EventMessageArgs(message));
                target.RetryHandlingMessages();
            }
        }

        [Test]
        public void ShouldRetryForForecastEvent()
        {
	        var message = new EventMessage(Guid.NewGuid(),
		        DateTime.UtcNow,
		        DateTime.UtcNow,
		        Guid.NewGuid(),
		        Guid.NewGuid(),
		        Guid.NewGuid(),
		        typeof (IForecastData).Name,
		        DomainUpdateType.Insert,
		        string.Empty,
		        DateTime.UtcNow);
            
            using (mocks.Record())
            {
                Expect.Call(view.InvokeRequired).Return(false);
				Expect.Call(() => forecastCommand.Execute(message)).Throw(new DataSourceException());
                Expect.Call(view.ShowBackgroundDataSourceError);

            	Expect.Call(() => forecastCommand.Execute(message));
                Expect.Call(view.HideBackgroundDataSourceError);
            }
            using (mocks.Playback())
            {
				target.OnEventForecastDataMessageHandler(null, new EventMessageArgs(message));
                target.RetryHandlingMessages();
            }
        }

        [Test]
        public void ShouldRetryForScheduleEvent()
        {
            var objectId = Guid.NewGuid();
	        var message = new EventMessage(Guid.NewGuid(),
		        DateTime.UtcNow,
		        DateTime.UtcNow,
		        Guid.NewGuid(),
		        Guid.NewGuid(),
		        objectId,
		        typeof (IPersonAssignment).Name,
		        DomainUpdateType.Delete,
		        string.Empty,
		        DateTime.UtcNow) {InterfaceType = typeof (IPersonAssignment)};

            using (mocks.Record())
            {
                Expect.Call(view.InvokeRequired).Return(false);
				Expect.Call(() => scheduleCommand.Execute(message)).Throw(new DataSourceException());
                Expect.Call(view.ShowBackgroundDataSourceError);

				Expect.Call(() => scheduleCommand.Execute(message));
                Expect.Call(view.HideBackgroundDataSourceError);
            }
            using (mocks.Playback())
            {
                target.OnEventScheduleMessageHandler(null,
                                                      new EventMessageArgs(message));
                target.RetryHandlingMessages();
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();

            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            target.Dispose();
        }
    }
}
