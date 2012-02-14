using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Ccc.Domain.Security.ActiveDirectory
{
    /// <summary>
    /// ActiveDirectoryGroup Mapper
    /// </summary>
    public static class ActiveDirectoryGroupMapper
    {
        /// <summary>
        /// Get all group properties.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<string> AllGroupProperties()
        {
            List<string> properties = new List<string>();
            properties.Add(CHANGED);
            properties.Add(COMMONNAME);
            properties.Add(CREATED);
            properties.Add(DISTINGUISHEDNAME);
            properties.Add(MEMBER);
            properties.Add(MEMBEROF);
            properties.Add(OBJECTGUID);
            properties.Add(OBJECTSID);
            properties.Add(SAMACCOUNTNAME);
            return properties;
        }

        /// <summary>
        /// (DateTime, 'whenChanged') Changed - Shows the last time the object was updated/modified.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CHANGED")]
        public const string CHANGED = "whenChanged";

        /// <summary>
        /// (string, 'cn') Common Name - The object's Short Path Name in Active Directory.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "COMMONNAME")]
        public const string COMMONNAME = "cn";

        /// <summary>
        /// (DateTime, 'whenCreated') Created - Shows the time the object was created.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CREATED")]
        public const string CREATED = "whenCreated";

        /// <summary>
        /// (string, 'distinguishedName') Distinguished Name - The object's Full Path Name in Active Directory.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DISTINGUISHEDNAME")]
        public const string DISTINGUISHEDNAME = "distinguishedName";

        /// <summary>
        /// (string[], 'member') Member - The groups that are a member of this group..
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MEMBER")]
        public const string MEMBER = "member";

        /// <summary>
        /// (string[], 'memberOf') Member Of - The groups that this group is a direct member of.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MEMBEROF")]
        public const string MEMBEROF = "memberOf";

        /// <summary>
        /// (byte[], 'objectGuid') Object GUID - The Unique Object Identifier.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OBJECTGUID")]
        public const string OBJECTGUID = "objectGuid";

        /// <summary>
        /// (byte[], 'objectSid') Object SID - The Unique Object Identifier.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OBJECTSID")]
        public const string OBJECTSID = "objectSid";

        /// <summary>
        /// (string, 'sAMAccountName') SAM Account Name - The Group UserName.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAMACCOUNTNAME")]
        public const string SAMACCOUNTNAME = "sAMAccountName";

        /// <summary>
        /// (string, 'tokenGroups') Token Groups - The recursive group membership list. (CANNOT BE RETRIEVED THROUGH DIRECTORYSEARCHER)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TOKENGROUPS")]
        public const string TOKENGROUPS = "tokenGroups";


        // NON-ACTIVE DIRECTORY BASED PROPERTIES

        /// <summary>
        /// (string, 'objectGuidString') Object GUID String - String Value of the (byte[]) Object GUID.
        /// [NOTE: Use 'OBJECTGUID' for keys/searching outside of the parent 'User' class.]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OBJECTGUIDSTRING")]
        public const string OBJECTGUIDSTRING = "objectGuidString";

        /// <summary>
        /// (string, 'objectSidString') Object SID String - String Value of the (byte[]) Object SID.
        /// [NOTE: Use 'OBJECTSID' for keys/searching outside of the parent 'Group' class.]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OBJECTSIDSTRING")]
        public const string OBJECTSIDSTRING = "objectSidString";

        /// <summary>
        /// (Dictionary, 'member') MemberActions - The Actions to Perform on Groups that are a member of this group..
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MEMBERACTIONS")]
        public const string MEMBERACTIONS = "MemberActions";
    }

}
