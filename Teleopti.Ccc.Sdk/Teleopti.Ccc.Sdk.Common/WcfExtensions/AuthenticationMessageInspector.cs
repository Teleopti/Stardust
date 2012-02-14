using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Teleopti.Ccc.Sdk.Common.WcfExtensions
{
    public static class AuthenticationMessageHeader
    {
        [ThreadStatic]
        private static string _userName;
        [ThreadStatic]
        private static string _password;
        [ThreadStatic]
        private static string _dataSource;
        [ThreadStatic]
        private static Guid _businessUnit;
        [ThreadStatic]
        private static bool _useWindowsIdentity;

        public static string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public static string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public static string DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        public static Guid BusinessUnit
        {
            get { return _businessUnit; }
            set { _businessUnit = value; }
        }

        public static bool UseWindowsIdentity
        {
            get { return _useWindowsIdentity; }
            set { _useWindowsIdentity = value; }
        }
    }

    public class AuthenticationMessageInspector : IClientMessageInspector
    {
        private static AuthenticationMessageInspector _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMessageInspector"/> class.
        /// </summary>
        private AuthenticationMessageInspector()
        {
        }

        /// <summary>
        /// Gets the singleton <see cref="AuthenticationMessageInspector" /> instance.
        /// </summary>
        public static AuthenticationMessageInspector Instance
        {
            get { return _instance ?? (_instance = new AuthenticationMessageInspector()); }
        }

        /// <summary>
        /// Inspects a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param>
        /// <param name="correlationState">Correlation state data.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        /// <summary>
        /// Inspects a message before a request message is sent to a service.
        /// </summary>
        /// <param name="request">The message to be sent to the service.</param>
        /// <param name="channel">The client object channel.</param>
        /// <returns>
        /// <strong>Null</strong> since no message correlation is used.
        /// </returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            request.Headers.Add(new TeleoptiAuthenticationHeader(AuthenticationMessageHeader.UserName,
                                                                 AuthenticationMessageHeader.Password,
                                                                 AuthenticationMessageHeader.DataSource,
                                                                 AuthenticationMessageHeader.BusinessUnit)
                                    {UseWindowsIdentity = AuthenticationMessageHeader.UseWindowsIdentity});

            return null;
        }
    }

    /// <summary>
    /// Adds the singleton instance of <see cref="AuthenticationMessageInspector"/> to an endpoint on the client.
    /// </summary>
    public class AuthenticationMessageEndpointBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            return;
        }

        /// <summary>
        /// Adds the singleton of the <see cref="AuthenticationMessageInspector"/> class to the client endpoint's message inspectors.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(AuthenticationMessageInspector.Instance);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            return;
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            return;
        }
    }

    public class AuthenticationMessageBehaviorExtension : BehaviorExtensionElement
    {
        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(AuthenticationMessageEndpointBehavior); }
        }

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        protected override object CreateBehavior()
        {
            return new AuthenticationMessageEndpointBehavior();
        }
    }
}