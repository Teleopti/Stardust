using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.ActiveDirectory
{
            /// <summary>
        /// The Property Keys for the Active Directory User Object.
        /// </summary>
    public static class ActiveDirectoryUserMapper
    {
        /// <summary>
        /// Alls the user properties.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
            public static List<string> AllUserProperties()
            {
                List<string> properties = new List<string>();
                properties.Add(ACCOUNTCONTROL);
                properties.Add(ASSISTANT);
                properties.Add(CELLPHONE);
                properties.Add(CHANGED);
                properties.Add(CITY);
                properties.Add(COMMONNAME);
                properties.Add(COMPANY);
                properties.Add(CREATED);
                properties.Add(DEPARTMENT);
                properties.Add(DESCRIPTION);
                properties.Add(DISTINGUISHEDNAME);
                properties.Add(EMAILADDRESS);
                properties.Add(EMPLOYEEID);
                properties.Add(FAXNUMBER);
                properties.Add(FIRSTNAME);
                properties.Add(FULLDISPLAYNAME);
                properties.Add(HOMEDIRECTORY);
                properties.Add(HOMEDRIVE);
                properties.Add(HOMEPHONE);
                properties.Add(LASTNAME);
                properties.Add(LOGONCOUNT);
                properties.Add(MEMBEROF);
                properties.Add(MIDDLEINITAL);
                properties.Add(NOTES);
                properties.Add(OBJECTSID);
                properties.Add(OBJECTGUID);
                properties.Add(OFFICE);
                properties.Add(PASSWORDLASTSET);
                properties.Add(POBOX);
                properties.Add(PROFILEPATH);
                properties.Add(SAMACCOUNTNAME);
                properties.Add(SCRIPTPATH);
                properties.Add(STATE);
                properties.Add(STREETADDRESS);
                properties.Add(USERPRINCIPALNAME);
                properties.Add(ZIPCODE);
                return properties;
            }

            /// <summary>
            /// (int, 'userAccountControl') Account Control - Contains user account properties.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ACCOUNTCONTROL")]
            public const string ACCOUNTCONTROL = "userAccountControl";

            /// <summary>
            /// (string, 'assistant') Assistant - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ASSISTANT")]
            public const string ASSISTANT = "assistant";

            /// <summary>
            /// (string, 'mobile') Cell Phone - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CELLPHONE")]
            public const string CELLPHONE = "mobile";

            /// <summary>
            /// (DateTime, 'whenChanged') Changed - Shows the last time the object was updated/modified.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CHANGED")]
            public const string CHANGED = "whenChanged";

            /// <summary>
            /// (string, 'l') City - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CITY")]
            public const string CITY = "l";

            /// <summary>
            /// (string, 'cn') Common Name - The object's Short Path Name in Active Directory.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "COMMONNAME")]
            public const string COMMONNAME = "cn";

            /// <summary>
            /// (string, 'company') Company - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "COMPANY")]
            public const string COMPANY = "company";

            /// <summary>
            /// (DateTime, 'whenCreated') Created - Shows the time the object was created.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "CREATED")]
            public const string CREATED = "whenCreated";

            /// <summary>
            /// (string, 'department') Department - Address Book Data Field. (STORES ENCRYPTED PASSWORD)
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DEPARTMENT")]
            public const string DEPARTMENT = "department";

            /// <summary>
            /// (string, 'description') Description - Short Description of the object.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DESCRIPTION")]
            public const string DESCRIPTION = "description";

            /// <summary>
            /// (string, 'distinguishedName') Distinguished Name - The object's Full Path Name in Active Directory.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DISTINGUISHEDNAME")]
            public const string DISTINGUISHEDNAME = "distinguishedName";

            /// <summary>
            /// (string, 'mail') Email Address - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "EMAILADDRESS")]
            public const string EMAILADDRESS = "mail";

            /// <summary>
            /// (string, 'employeeId') Employee ID - Hidden Data Field (FORMAT SHOULD BE #####).
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "EMPLOYEEID")]
            public const string EMPLOYEEID = "employeeId";

            /// <summary>
            /// (string, 'facsimileTelephoneNumber') Fax Number - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FAXNUMBER")]
            public const string FAXNUMBER = "facsimileTelephoneNumber";

            /// <summary>
            /// (string, 'givenName') First Name - The First Name of the object.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FIRSTNAME")]
            public const string FIRSTNAME = "givenName";

            /// <summary>
            /// (string, 'displayName') Full Display Name - The name shown on Windows Taskbar.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FULLDISPLAYNAME")]
            public const string FULLDISPLAYNAME = "displayName";

            /// <summary>
            /// (string, 'homeDirectory') Home Directory - The 'Data Path' Directory Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HOMEDIRECTORY")]
            public const string HOMEDIRECTORY = "homeDirectory";

            /// <summary>
            /// (string, 'homeDrive') Home Drive - The Drive Letter to assign to the Home Directory.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HOMEDRIVE")]
            public const string HOMEDRIVE = "homeDrive";

            /// <summary>
            /// (string, 'homePhone') Home Phone - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HOMEPHONE")]
            public const string HOMEPHONE = "homePhone";

            /// <summary>
            /// (string, 'sn') Last Name - The Last Name of the object.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LASTNAME")]
            public const string LASTNAME = "sn";

            /// <summary>
            /// (int, 'logonCount') Logon Count - Shows how many logons a user has.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LOGONCOUNT")]
            public const string LOGONCOUNT = "logonCount";

            /// <summary>
            /// (string[], 'memberOf') Member Of - The groups that an object is a direct member of.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MEMBEROF")]
            public const string MEMBEROF = "memberOf";

            /// <summary>
            /// (string, 'initials') Middle Initial - The Middle Initial of the object.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MIDDLEINITAL")]
            public const string MIDDLEINITAL = "initials";

            /// <summary>
            /// (string, 'info') Notes - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "NOTES")]
            public const string NOTES = "info";

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
            /// (string, 'physicalDeliveryOfficeName') Office - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OFFICE")]
            public const string OFFICE = "physicalDeliveryOfficeName";

            /// <summary>
            /// (int, 'pwdLastSet') Password Last Set - Used for Password Changing.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PASSWORDLASTSET")]
            public const string PASSWORDLASTSET = "pwdLastSet";

            /// <summary>
            /// (string, 'postOfficeBox') P.O. Box - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "POBOX")]
            public const string POBOX = "postOfficeBox";

            /// <summary>
            /// (string, 'profilePath') Profile Path - The 'Profile Path' Directory Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PROFILEPATH")]
            public const string PROFILEPATH = "profilePath";

            /// <summary>
            /// (string, 'sAMAccountName') SAM Account Name - The User's Login UserName.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAMACCOUNTNAME")]
            public const string SAMACCOUNTNAME = "sAMAccountName";

            /// <summary>
            /// (string, 'scriptPath') Script Path - The Logon Script Path.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SCRIPTPATH")]
            public const string SCRIPTPATH = "scriptPath";

            /// <summary>
            /// (string, 'st') State - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "STATE")]
            public const string STATE = "st";

            /// <summary>
            /// (string, 'streetAddress') Street Address - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "STREETADDRESS")]
            public const string STREETADDRESS = "streetAddress";

            /// <summary>
            /// (string, 'tokenGroups') Token Groups - The recursive group membership list. (CANNOT BE RETRIEVED THROUGH DIRECTORYSEARCHER)
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TOKENGROUPS")]
            public const string TOKENGROUPS = "tokenGroups";

            /// <summary>
            /// (string, 'userPrincipalName') User Principal Name - The Full Login UserName with Domain.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "USERPRINCIPALNAME")]
            public const string USERPRINCIPALNAME = "userPrincipalName";

            /// <summary>
            /// (string, 'postalCode') Zip Code - Address Book Data Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ZIPCODE")]
            public const string ZIPCODE = "postalCode";

            /// <summary>
            /// (string, 'objectGuidString') Object GUID String - String Value of the (byte[]) Object GUID.
            /// [NOTE: Use 'OBJECTGUID' for keys/searching outside of the parent 'User' class.]
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OBJECTGUIDSTRING")]
            public const string OBJECTGUIDSTRING = "objectGuidString";

            /// <summary>
            /// (string, 'objectSidString') Object SID String - String Value of the (byte[]) Object SID.
            /// [NOTE: Use 'OBJECTSID' for keys/searching outside of the parent 'User' class.]
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OBJECTSIDSTRING")]
            public const string OBJECTSIDSTRING = "objectSidString";

            /// <summary>
            /// (string[], 'Path') Path - The Calculated Path of the Object (Deals with the Full Path).
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PATH")]
            public const string PATH = "Path";
        }

    }
