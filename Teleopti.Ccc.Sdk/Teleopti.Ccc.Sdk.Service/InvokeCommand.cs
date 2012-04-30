using System.Reflection;
using System.ServiceModel;
using Autofac;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

namespace Teleopti.Ccc.Sdk.WcfService
{
	public class InvokeCommand : IInvokeCommand
	{
		private readonly ILifetimeScope _lifetimeScope;

		public InvokeCommand(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CommandResultDto Invoke(CommandDto command)
		{
			var handler = _lifetimeScope.Resolve(typeof(IHandleCommand<>).MakeGenericType(new[] { command.GetType() }));
			var method = handler.GetType().GetMethod("Handle");
            CommandResultDto result = null;
            try
            {
                result = (CommandResultDto)method.Invoke(handler, new[] { command });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw new FaultException(e.InnerException.Message);
            }
		    return result;
		}
	}
}