﻿using System;
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
	        var buId = string.Empty;
	        var queryString = _context.Current().Request.QueryString;
			if (queryString != null)
				buId = queryString["BusinessUnitId"];
			var headers = _context.Current().Request.Headers;
			if (headers != null)
				buId = headers["X-Business-Unit-Filter"];
			
			if (string.IsNullOrEmpty(buId)) return;
			var id = Guid.Parse(buId);
            _disposable = _overrider.OverrideWith(id);
        }

        public void OnAfterInvokation()
        {
	        if (_disposable != null)
	        {
				_disposable.Dispose();
				_disposable = null;
	        }
        }

    }

}