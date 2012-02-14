using System.Reflection;
using System.ServiceModel;
using Autofac;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
	public class InvokeCommand : IInvokeCommand
	{
		private readonly ILifetimeScope _lifetimeScope;

		public InvokeCommand(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

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