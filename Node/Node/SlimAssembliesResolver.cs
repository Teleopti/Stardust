﻿using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace Stardust.Node
{
    public class SlimAssembliesResolver : IAssembliesResolver
    {
        private readonly Assembly _assembly;

        public SlimAssembliesResolver(Assembly assembly)
        {
            _assembly = assembly;
        }

        public ICollection<Assembly> GetAssemblies()
        {
            return new[] { _assembly };
        }
    }
}