using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.IocConfig
{
    public class RuleSetModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new CreateWorkShiftsFromTemplate())
                .As<ICreateWorkShiftsFromTemplate>()
                .SingleInstance();
            builder.Register(c => new ShiftCreatorService(c.Resolve<ICreateWorkShiftsFromTemplate>()))
                .As<IShiftCreatorService>()
                .SingleInstance();
            builder.Register(c => new RuleSetProjectionService(c.Resolve<IShiftCreatorService>()))
                .Named<IRuleSetProjectionService>("UnCached")
                .SingleInstance();
            builder.Register(c => c.Resolve<IMbCacheFactory>().Create<IRuleSetProjectionService>())
                .As<IRuleSetProjectionService>()
                .SingleInstance();
        }
    }
}