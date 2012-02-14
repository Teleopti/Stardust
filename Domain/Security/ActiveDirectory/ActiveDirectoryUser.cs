using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Ccc.Domain.Security.ActiveDirectory
{
    /// <summary>
    /// Represents a Active Directory User.
    /// </summary>
    public class ActiveDirectoryUser
    {

        #region Constants

        /// <summary>
        /// (int, '2') Disable Account Flag - Value to match with Account Control.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "FLAG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FLAG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DISABLEACCOUNT")]
        public const int FLAG_DISABLEACCOUNT = 2;

        /// <summary>
        /// (int, '512') Normal Account Flag - Value to match with Account Control.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "NORMALACCOUNT")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "FLAG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FLAG")]
        public const int FLAG_NORMALACCOUNT = 512;

        /// <summary>
        /// (int, '65536') Don't Expire Password Flag - Value to match with Account Control.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "FLAG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FLAG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DONTEXPIREPASSWORD")]
        public const int FLAG_DONTEXPIREPASSWORD = 65536;

        #endregion

        #region Variables

        private int _accountControl;
        private string _assistant;
        private string _cellPhone;
        private string _city;
        private DateTime _changed = DateTime.MinValue;
        private string _commonName;
        private string _company;
        private DateTime _created = DateTime.MinValue;
        private string _department;
        private string _description;
        private string _distinguishedName;
        private string _emailAddress;
        private string _employeeID;
        private string _faxNumber;
        private string _firstName;
        private string _fullDisplayName;
        private string _homeDirectory;
        private string _homeDrive;
        private string _homePhone;
        private string _lastName;
        private int _logonCount = -1;
        private List<string> _memberOf;
        private string _middleInitial;
        private string _notes;
        private byte[] _objectGUID;
        private string _objectGUIDString;
        private byte[] _objectSID;
        private string _objectSIDString;
        private string _office;
        private int _passwordLastSet;
        private string _POBox;
        private string _profilePath;
        private string _SAMAccountName;
        private string _scriptPath;
        private string _state;
        private string _streetAddress;
        private List<ActiveDirectoryGroup> _tokenGroups = new List<ActiveDirectoryGroup>();
        private string _userPrincipalName;
        private string _zipCode;
        private string _path;

        #endregion

        #region Constructors

        /// <summary>
        /// Represents a Active Directory User.
        /// </summary>
        public ActiveDirectoryUser() 
        {
            AccountControl = FLAG_NORMALACCOUNT;
        }

        #endregion

        #region Interface

        /// <summary>
        /// Account Control - Contains user account UserMapper.
        /// </summary>
        public int AccountControl
        {
            get { return _accountControl; }
            set { _accountControl = value; }
        }

        /// <summary>
        /// Assistant - Address Book Data Field.
        /// </summary>
        public string Assistant
        {
            get { return _assistant; }
            set { _assistant = value; }
        }

        /// <summary>
        /// Cell Phone - Address Book Data Field.
        /// </summary>
        public string CellPhone
        {
            get { return _cellPhone; }
            set { _cellPhone = value; }
        }

        /// <summary>
        /// Changed - Shows the last time the user was updated/modified.
        /// </summary>
        public DateTime Changed
        {
            get { return _changed; }
            set { _changed = value; }
        }

        /// <summary>
        /// City - Address Book Data Field.
        /// </summary>
        public string City
        {
            get { return _city; }
            set { _city = value; }
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
        /// Company - Address Book Data Field. (STORES BIRTHDATE)
        /// </summary>
        public string Company
        {
            get { return _company; }
            set { _company = value; }
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
        /// Department - Address Book Data Field. (STORES ENCRYPTED PASSWORD)
        /// </summary>
        public string Department
        {
            get { return _department; }
            set { _department = value; }
        }

        /// <summary>
        /// Description - Short Description of the user.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Distinguished Name - The user's Full Path in Active Directory.
        /// </summary>
        public string DistinguishedName
        {
            get { return _distinguishedName; }
            set 
            { 
                _distinguishedName = value;
                Path = CalculatePath(value);
            }
        }

        /// <summary>
        /// Email Address - Address Book Data Field.
        /// </summary>
        public string EmailAddress
        {
            get { return _emailAddress; }
            set { _emailAddress = value; }
        }

        /// <summary>
        /// Employee ID - Hidden Data Field containing the Employee's Identification String.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public string EmployeeID
        {
            get { return _employeeID; }
            set { _employeeID = value; }
        }

        /// <summary>
        /// Fax Number - Address Book Data Field.
        /// </summary>
        public string FaxNumber
        {
            get { return _faxNumber; }
            set { _faxNumber = value; }
        }

        /// <summary>
        /// First Name - The First Name of the user.
        /// </summary>
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        /// <summary>
        /// Full Display Name - Field containing the Full Name shown on the Windows Taskbar in Windows XP.
        /// </summary>
        public string FullDisplayName
        {
            get { return _fullDisplayName; }
            set { _fullDisplayName = value; }
        }

        /// <summary>
        /// Full Name - Field combining the FirstName, MiddleInitial and LastName fields separated by spaces.
        /// </summary>
        public string FullName
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(FirstName);
                sb.Append(sb.Length > 0 && !string.IsNullOrEmpty(MiddleInitial) ? " " : null);
                sb.Append(MiddleInitial);
                sb.Append(sb.Length > 0 && !string.IsNullOrEmpty(LastName) ? " " : null);
                sb.Append(LastName);
                return sb.ToString();
            }

        }

        /// <summary>
        /// TokenGroups - The Recursive Security Group Membership List of the user.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<ActiveDirectoryGroup> TokenGroups
        {
            get { return _tokenGroups; }
        }

        /// <summary>
        /// Home Directory - The 'Data Path' Directory Field.
        /// </summary>
        public string HomeDirectory
        {
            get { return _homeDirectory; }
            set { _homeDirectory = value; }
        }

        /// <summary>
        /// Home Drive - The Drive Letter assigned to the Home Directory path.
        /// </summary>
        public string HomeDrive
        {
            get { return _homeDrive; }
            set { _homeDrive = value; }
        }

        /// <summary>
        /// Home Phone - Address Book Data Field.
        /// </summary>
        public string HomePhone
        {
            get { return _homePhone; }
            set { _homePhone = value; }
        }

        /// <summary>
        /// Last Name - The Last Name of the user.
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        /// <summary>
        /// Logon Count - Shows the number of logons for the user.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Logon")]
        public int LogonCount
        {
            get { return _logonCount; }
            set { _logonCount = value; }
        }

        /// <summary>
        /// Member Of - The Groups the user is a direct member of.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> MemberOf
        {
            get 
            { 
                if (_memberOf == null)
                    _memberOf = new List<string>();
                return _memberOf; 
            }
        }

        /// <summary>
        /// Fax Number - Address Book Data Field.
        /// </summary>
        public string MiddleInitial
        {
            get { return _middleInitial; }
            set { _middleInitial = value; }
        }

        /// <summary>
        /// Notes - Expanded Description Field.
        /// </summary>
        public string Notes
        {
            get { return _notes; }
            set { _notes = value; }
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
            protected set { _objectSIDString = value; }
        }

        /// <summary>
        /// Office - Address Book Data Field.
        /// </summary>
        public string Office
        {
            get { return _office; }
            set { _office = value; }
        }

        /// <summary>
        /// Password Last Set - An integer value representing the last password reset.
        /// </summary>
        public int PasswordLastSet
        {
            get { return _passwordLastSet; }
            set { _passwordLastSet = value; }
        }

        /// <summary>
        /// P.O. Box - Address Book Data Field.
        /// </summary>
        public string POBox
        {
            get { return _POBox; }
            set { _POBox = value; }
        }

        /// <summary>
        /// Office - The 'Profile Path' Field.
        /// </summary>
        public string ProfilePath
        {
            get { return _profilePath; }
            set { _profilePath = value; }
        }

        /// <summary>
        /// SAM Account Name - The User's Login Username. (without domain info)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM")]
        public string SAMAccountName
        {
            get { return _SAMAccountName; }
            set { _SAMAccountName = value; }
        }

        /// <summary>
        /// Script Path - The Logon Script Path.
        /// </summary>
        public string ScriptPath
        {
            get { return _scriptPath; }
            set { _scriptPath = value; }
        }

        /// <summary>
        /// State - Address Book Data Field.
        /// </summary>
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// Street Address - Address Book Data Field.
        /// </summary>
        public string StreetAddress
        {
            get { return _streetAddress; }
            set { _streetAddress = value; }
        }

        /// <summary>
        /// User Principal Name - The Full Logon Username. (with domain info)
        /// </summary>
        public string UserPrincipalName
        {
            get { return _userPrincipalName; }
            set { _userPrincipalName = value; }
        }

        /// <summary>
        /// Zip Code - Address Book Data Field.
        /// </summary>
        public string ZipCode
        {
            get { return _zipCode; }
            set { _zipCode = value; }
        }

        /// <summary>
        /// The Account State Of the User.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return (AccountControl & FLAG_DISABLEACCOUNT) != FLAG_DISABLEACCOUNT;
            }
            set
            {
                if (value)
                    AccountControl &= ~FLAG_DISABLEACCOUNT;
                else
                    AccountControl |= FLAG_DISABLEACCOUNT;
            }
        }

        /// <summary>
        /// Gets/Sets the Flag 'Password Never Expires'.
        /// </summary>
        public bool PasswordNeverExpires
        {
            get
            {
                return (AccountControl & FLAG_DONTEXPIREPASSWORD) == FLAG_DONTEXPIREPASSWORD;
            }
            set
            {
                if (!value)
                    AccountControl &= ~FLAG_DONTEXPIREPASSWORD;
                else
                    AccountControl |= FLAG_DONTEXPIREPASSWORD;
            }
        }

        /// <summary>
        /// Sets the Flag 'User Must Change Password on Next Logon'.
        /// </summary>
        public bool MustChangePasswordOnNextLogOn
        {
            get { return PasswordLastSet == 0; }
            set { PasswordLastSet = (value ? 0 : -1); }
        }

        /// <summary>
        /// Gets the Path of the Object in Active Directory (Reverse Order of DistinguishedName, Names only, Separated by '\' like a Folder Path).
        /// </summary>
        public string Path
        {
            get { return _path; }
            protected set { _path = value; }
        }

        #endregion

        #region Local Methods

   
        /// <summary>
        /// Reformats the DistinguishedName Property into a Folder-like Path.
        /// </summary>
        /// <returns>The Path Fields, in Order from Root to User, separated by a '\'.</returns>
        private string CalculatePath(string distinguishedName)
        {
            StringBuilder sb = new StringBuilder();
            List<string> Parts = new List<string>();

            if (!string.IsNullOrEmpty(distinguishedName))
            {
                Parts.AddRange(DistinguishedName.Split(','));
                Parts.Reverse();

                foreach (string Part in Parts)
                {
                    if (sb.Length > 0) sb.Append(@"\");
                    sb.Append(Part.Trim().Substring(Part.IndexOf('=') + 1));
                }
            }
            
            return sb.ToString();
        }

        #endregion

    }
}
