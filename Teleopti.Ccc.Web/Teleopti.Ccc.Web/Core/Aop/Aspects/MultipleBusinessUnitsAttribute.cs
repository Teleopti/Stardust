using Teleopti.Ccc.Web.Core.Aop.Core;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
    public sealed class MultipleBusinessUnitsAttribute : ResolvedAspectAttribute
    {
        public MultipleBusinessUnitsAttribute() : base(typeof(MultipleBusinessUnitsAspect)) { }
    }
}