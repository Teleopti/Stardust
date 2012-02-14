using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    internal abstract class ModuleConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ModuleConverter));
        private readonly ICccTimeZoneInfo _timeZoneInfo;
        private readonly MappedObjectPair _mappedObjectPair;
        private readonly DateTimePeriod _period;

        public ModuleConverter(MappedObjectPair mappedObjectPair, 
                               DateTimePeriod period,
                               ICccTimeZoneInfo timeZoneInfo)
        {
            _timeZoneInfo = timeZoneInfo;
            _period = period;
            _mappedObjectPair = mappedObjectPair;
        }

        internal void Convert()
        {
            checkDependencies();
            Logger.InfoFormat("Start to convert {0}...", ModuleName);
            GroupConvert();
            Logger.InfoFormat("Finished converting {0}", ModuleName);
            ModuleChecker.Instance.MarkAsRun(GetType());
        }

        private void checkDependencies()
        {
            IList<Type> missingConverters = new List<Type>();
            foreach (Type converter in DependedOn)
            {
                if(!ModuleChecker.Instance.RunModuleCheckerCollection.Contains(converter))
                    missingConverters.Add(converter);
            }
            if(missingConverters.Count!=0)
            {
                StringBuilder str = new StringBuilder();
                foreach (Type converter in missingConverters)
                {
                    str.Append(converter.Name + ", ");
                }
                str.Remove(str.Length - 2, 2);
                throw new InvalidOperationException("Cannot execute module converter " + ModuleName +
                                                    ", you need to run [" + str + "] first.");
            }

        }

        protected abstract string ModuleName { get; }

        protected abstract IEnumerable<Type> DependedOn { get;}

        protected ICccTimeZoneInfo TimeZoneInfo
        {
            get { return _timeZoneInfo; }
        }

        protected MappedObjectPair MappedObjectPair
        {
            get { return _mappedObjectPair; }
        }

        protected DateTimePeriod Period
        {
            get { return _period; }
        }

        protected abstract void GroupConvert();
    }
}