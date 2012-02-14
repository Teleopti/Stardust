using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Grouping
{
    [Serializable]
    public class TreeViewDictionary : Dictionary<TabPageAdv, TreeViewAdv>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewDictionary"/> class.
        /// </summary>
        public TreeViewDictionary() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewDictionary"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected TreeViewDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
