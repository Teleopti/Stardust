using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    internal class ModuleChecker
    {
        private readonly IList<Type> _runModuleCheckerCollection;
        private readonly static ModuleChecker _singleton = new ModuleChecker();

        private ModuleChecker()
        {
            _runModuleCheckerCollection = new List<Type>();
        }

        public static ModuleChecker Instance
        {
            get
            {
                return _singleton;
            }
        }

        internal void MarkAsRun(Type module)
        {
            InParameter.NotNull("module", module);
            _runModuleCheckerCollection.Add(module);
        }

        public ReadOnlyCollection<Type> RunModuleCheckerCollection
        {
            get { return new ReadOnlyCollection<Type>(_runModuleCheckerCollection); }
        }
    }
}
