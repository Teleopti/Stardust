using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Core.Aop.Aspects
{
    public class MultipleBusinessUnitsAspect : IAspect
    {
        private readonly IBusinessUnitFilterOverrider _overrider;
        private readonly ICurrentHttpContext _context;
        private IDisposable _disposable;

        public MultipleBusinessUnitsAspect(IBusinessUnitFilterOverrider overrider, ICurrentHttpContext context)
        {
            _overrider = overrider;
            _context = context;
        }

        public void OnBeforeInvokation()
        {
            var value = _context.Current().Request.Headers["x-business-unit-filter"];
            var id = Guid.Parse(value);
            _disposable = _overrider.OverrideWith(id);
        }

        public void OnAfterInvokation()
        {
            _disposable.Dispose();
            _disposable = null;
        }

    }

}