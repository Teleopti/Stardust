using System;
using System.IO;
using System.Threading;
using System.Transactions;
using log4net;
using Rhino.Queues.Model;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.RhinoQueues;
using Rhino.ServiceBus.Transport;
using Teleopti.Ccc.Sdk.ServiceBus.Messages;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ErrorNotification : IServiceBusAware
    {
        private readonly IMessageSerializer _messageSerializer;
        private volatile bool shouldContinue;
        private RhinoQueuesTransport _transport;
        private readonly ILog logger = LogManager.GetLogger(typeof(ErrorNotification));
        private Thread _thread;
        private IServiceBus _bus;

        public ErrorNotification(ITransport transport, IMessageSerializer messageSerializer)
        {
            _messageSerializer = messageSerializer;
            _transport = transport as RhinoQueuesTransport;
        }

        public void BusStarting(IServiceBus bus)
        {
            _bus = bus;
            if (_transport!=null)
                _transport.Started += OnTransportStart;
        }

        public void BusStarted(IServiceBus bus)
        {
        }

        private void OnTransportStart()
        {
            logger.Info("Starting Error Notifier");
            if (_transport == null) return;
            
            _thread = new Thread(RecieveError)
                          {
                              Name = "Rhino Service Bus Error Notifier Worker Thread",
                              IsBackground = true
                          };
            shouldContinue = true;
            _thread.Start();
            logger.Info("Error Notifier thread started.");
        }

        private void RecieveError()
        {
            while (shouldContinue)
            {
                try
                {
                    _transport.Queue.Peek(SubQueue.Errors.ToString());
                }
                    catch(ObjectDisposedException)
                    {
                        logger.InfoFormat("Could not read error message from error queue for Uri {0} due to object disposed. No more error notification.",
                                      _transport.Endpoint);
                        break;
                    }
                catch (Exception)
                {
                    logger.InfoFormat("Could not read error message from error queue for Uri {0}.",
                                      _transport.Endpoint);
                    Thread.SpinWait(3000000);
                    continue;
                }

                if (shouldContinue == false) return;

                Message message;
                try
                {
                    using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        message = _transport.Queue.Receive(SubQueue.Errors.ToString(), TimeSpan.FromSeconds(1));
                        tx.Complete();
                    }
                }
                catch (Exception e)
                {
                    logger.Error(
                        "An error occured while recieving an error message, shutting down message processing thread", e);
                    return;
                }

                try
                {
                    logger.InfoFormat("Starting to handle message {0} on {1}",
                                      message.Id, _transport.Endpoint);

                    ProcessMessage(message);
                }
                catch (Exception exception)
                {
                    logger.Info("Could not process message", exception);
                }
            }
        }

        private void ProcessMessage(Message message)
        {
            logger.InfoFormat("{0}",message);
            object messageData;
            try
            {
                using (var ms = new MemoryStream(message.Data))
                {
                    messageData = _messageSerializer.Deserialize(ms)[0];
                }
            }
            catch(Exception)
            {
                messageData = System.Text.Encoding.Unicode.GetString(message.Data);
            }
            try
            {
                CustomErrorMessage errorMessage;
                if (message.Headers["source"]!=null)
                {
                    var messageId = message.Headers["id"];
                    var source = new Uri(message.Headers["source"]);
                    errorMessage = new CustomErrorMessage
                                       {
                                           Message = messageData,
                                           Destination = _transport.Endpoint.Uri,
                                           MessageId = messageId,
                                           TransportMessageId = message.Id.ToString(),
                                           Source = source
                                       };
                }
                else
                {
                    var messageId = message.Headers["correlation-id"];
                    errorMessage = new CustomErrorMessage
                    {
                        Message = messageData,
                        Destination = _transport.Endpoint.Uri,
                        MessageId = messageId,
                        TransportMessageId = message.Id.ToString(),
                    };
                }

                _bus.Notify(errorMessage);
            }
            catch (Exception e)
            {
                logger.Error("Failed to process error message", e);
                throw;
            }
        }

        public void BusDisposing(IServiceBus bus)
        {
            if (_transport!=null)
                _transport.Started -= OnTransportStart;
            shouldContinue = false;
            if (_thread!=null && _thread.IsAlive)
                _thread.Join(TimeSpan.FromSeconds(10));
        }

        public void BusDisposed(IServiceBus bus)
        {
            shouldContinue = false;
            _transport = null;
            _bus = null;
        }
    }
}
