using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2239:ProvideDeserializationMethodsForOptionalFields"), Serializable]
    public class VirtualSkill
    {
        private string _name;
        private readonly IList<Guid> _childSkills = new List<Guid>();
        [OptionalField]
        private Guid _id;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        
        public Guid Id
        {
            get{return _id;}
            set { _id = value; }
        }

        public IList<Guid> ChildSkills
        {
            get { return _childSkills; }
        }
    }
}