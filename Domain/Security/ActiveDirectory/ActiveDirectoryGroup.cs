using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.ActiveDirectory
{

    #region Enums

    /// <summary>
    /// The Action to Perform on the Group Member
    /// </summary>
    public enum MemberAction
    {
        /// <summary>
        /// No Change to the Group Member.
        /// </summary>
        None,

        /// <summary>
        /// Add Group Member to Group.
        /// </summary>
        Add,

        /// <summary>
        /// Remove Group Member from Group.
        /// </summary>
        Remove
    }

    #endregion

    /// <summary>
    /// Represents a Active Directory Group.
    /// </summary>
    public class ActiveDirectoryGroup
    {

        #region Variables

        private DateTime _changed = DateTime.MinValue;
        private DateTime _created = DateTime.MinValue;
        private string _commonName;
        private string _distinguishedName;
        private List<string> _memberOf = new List<string>();
        private SortedDictionary<string, MemberAction> _memberActions = new SortedDictionary<string, MemberAction>();
        private byte[] _objectGUID;
        private string _objectGUIDString;
        private byte[] _objectSID;
        private string _objectSIDString;
        private string _sAMAccountName;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryGroup"/> class.
        /// </summary>
        public ActiveDirectoryGroup()
        {
            //
        }

        #endregion

        #region Interface

        /// <summary>
        /// Changed - Shows the last time the user was updated/modified.
        /// </summary>
        public DateTime Changed
        {
            get { return _changed; }
            set { _changed = value; }
        }

        /// <summary>
        /// Common Name - The user's Short Path Name in Active Directory.
        /// This value cannot be set through this property.
        /// </summary>
        public string CommonName
        {
            get { return _commonName; }
            set { _commonName = value; }
        }

        /// <summary>
        /// Created - Shows the date/time the user was created.
        /// </summary>
        public DateTime Created
        {
            get { return _created; }
            set { _created = value; }
        }

        /// <summary>
        /// Distinguished Name - The user's Full Path in Active Directory.
        /// </summary>
        public string DistinguishedName
        {
            get { return _distinguishedName; }
            set { _distinguishedName = value; }
        }

        /// <summary>
        /// Member Of - The Groups the this group is a direct member of.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> MemberOf
        {
            get { return _memberOf; }
        }

        /// <summary>
        /// Members - The Groups that are a direct member of this group.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> Members
        {
            get
            {
                List<string> members = new List<string>();
                foreach (string member in MemberActions.Keys)
                    members.Add(member);
                return members;
            }
        }

        /// <summary>
        /// Member Actions - The Actions to Perform on Groups that are a direct member of this group.
        /// </summary>
        public SortedDictionary<string, MemberAction> MemberActions
        {
            get { return _memberActions; }
        }

        /// <summary>
        /// Object GUID - The GUID Identifier in byte[] format.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GUID")]
        public byte[] ObjectGUID
        {
            get { return _objectGUID; }
            set 
            {
                _objectGUID = value;
                ObjectGUIDString = ActiveDirectoryHelper.ConvertBytesToStringGUID(value);
            }
        }

        /// <summary>
        /// Object GUID - The GUID Identifier in string format.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GUID")]
        public string ObjectGUIDString
        {
            get { return _objectGUIDString; }
            protected set { _objectGUIDString = value; }
        }

        /// <summary>
        /// Object SID - The Unique Object Identifier in byte[] format.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID")]
        public byte[] ObjectSID
        {
            get { return _objectSID; }
            set 
            { 
                _objectSID = value;
                ObjectSIDString = ActiveDirectoryHelper.ConvertBytesToStringSid(ObjectSID);
            }
        }

        /// <summary>
        /// Object SID String - The Unique Object Identifier in string format.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID")]
        public string ObjectSIDString
        {
            get { return _objectSIDString; }
            protected set { _objectSIDString= value; }
        }

        /// <summary>
        /// SAM Account Name - The User's Login Username. (without domain info)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM")]
        public string SAMAccountName
        {
            get { return _sAMAccountName; }
            set { _sAMAccountName = value; }
        }

        #endregion

    }
}
