using Autofac;
using Microsoft.Practices.Composite.Events;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events
{
    public class EventAggregatorModule : Module
    {
        public const string GlobalTag = "global";
        public const string LocalTag = "local";

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new EventAggregator())
                .Named<IEventAggregator>(GlobalTag)
                .SingleInstance();
            builder.Register(c => new EventAggregator())
                .Named<IEventAggregator>(LocalTag)
                .InstancePerLifetimeScope();
        	builder.Register(c => c.ResolveNamed<IEventAggregator>(LocalTag)).As<IEventAggregator>();

			builder.Register(c =>
			                 	{
			                 		var ctx = c.Resolve<IComponentContext>();
			                 		return createLocator(ctx);
			                 	})
								.As<IEventAggregatorLocator>()
								.InstancePerDependency();
		}

        private static IEventAggregatorLocator createLocator(IComponentContext componentContext)
        { 
            return new eventAggregatorLocator(componentContext);
        }

        private class eventAggregatorLocator : IEventAggregatorLocator
        {
            private readonly IComponentContext _componentContext;

            public eventAggregatorLocator(IComponentContext componentContext)
            {
                _componentContext = componentContext;
            }

            public IEventAggregator GlobalAggregator()
            {


                return _componentContext.ResolveNamed<IEventAggregator>(GlobalTag);
            }

            public IEventAggregator LocalAggregator()
            {
                return _componentContext.ResolveNamed<IEventAggregator>(LocalTag);
            }
        }
    }
}