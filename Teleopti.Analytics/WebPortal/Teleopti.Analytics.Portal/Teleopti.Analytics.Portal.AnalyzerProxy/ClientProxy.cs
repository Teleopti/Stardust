using System;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;
using Teleopti.Analytics.Portal.AnalyzerProxy.Properties;
using log4net;


namespace Teleopti.Analytics.Portal.AnalyzerProxy
{
    public class ClientProxy : IClientProxy, IDisposable
    {
        private string _analyzerServer;
        private string _analyzerVirtualDirectory;
        private readonly string _olapServer;
        private readonly string _olapDatabase;
        private CatalogItem _analyzerDataSource;
        private CatalogItem _sharedReportsFolder;
        private readonly Analyzer2005 _az;
        private bool _disposed;
        private readonly ILog _log = LogManager.GetLogger(typeof(ClientProxy));
        private static SecurityContext _currentContext;

        // Hardcoded Ids of Folders node in "Analyzer 2007"
        private int rootNodeId = -1227456569;
        private int ShareReportId = 624687020;

        private enum FolderId
        {
            SharedFolder = 0,
            MyReport = 1
            //DataSourceFolder = 2,
            //RecycleBin = 3
        }

        private ClientProxy() { }

        public ClientProxy(string olapServer, string olapDatabase)
            : this()
				{
					ServicePointManager.ServerCertificateValidationCallback = ignoreInvalidCertificate;

            _log.Debug("Start of ClientProxy constructor");
            _az = new Analyzer2005();

            _log.DebugFormat("Analyzer URL will be set to: {0}", Settings.Default.Teleopti_Analytics_Portal_AnalyzerProxy_AnalyzerRef_Analyzer2005.Trim());
            _az.Url = Settings.Default.Teleopti_Analytics_Portal_AnalyzerProxy_AnalyzerRef_Analyzer2005;
            SetAnalyzerInfo();

            var cookies = new CookieContainer();
            string customSessionId = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
            if (HttpContext.Current != null)
            {
                customSessionId = HttpContext.Current.Session.SessionID;
            }
            cookies.Add(new Cookie("ASP.NET_SessionId", customSessionId, "/", _analyzerServer));
            _az.CookieContainer = cookies;

            _olapServer = olapServer;
            _olapDatabase = olapDatabase;

            AnalyzerAuthentication();
        }

	    private bool ignoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
	    {
		    return true;
	    }

	    private void AnalyzerAuthentication()
        {
            if (Settings.Default.PM_Authentication_Mode.Trim() == "Anonymous")
            {
                LogonUser();
            }
            else
            {
                // Windows authentication
                _az.PreAuthenticate = true;
                _az.Credentials = CredentialCache.DefaultCredentials;
                _log.Debug("Analyzer Windows authentication done.");
            }
        }

        private void LogonUser()
        {
            if (string.IsNullOrEmpty(Settings.Default.PM_Anonymous_User_Name.Trim())
                || string.IsNullOrEmpty(Settings.Default.PM_Anonymous_User_Password.Trim()))
            {
                _log.Debug("Analyzer Anonymous authentication failed. User and/or Password are missing!");
                throw new InvalidOperationException("Analyzer Anonymous authentication failed. User and/or Password are missing!");
            }
            string[] credentials = Settings.Default.PM_Anonymous_User_Name.Trim().Split("\\".ToCharArray());
            string domain = credentials[0];
            string user = credentials[1];
            _log.DebugFormat(
                "Analyzer Anonymous authentication will be done for user '{0}' and password with length of {1} characters.",
                Settings.Default.PM_Anonymous_User_Name.Trim(),
                Settings.Default.PM_Anonymous_User_Password.Trim().Length);
            SecurityContext securityContext = _az.LogonUser(domain, user, Settings.Default.PM_Anonymous_User_Password.Trim(), -1);
            if (!securityContext.Success)
            {
                string msgFailure = string.Format(CultureInfo.InvariantCulture,
                                                  "Analyzer Anonymous authentication failed. Message from Analyzer: '{0}'.",
                                                  securityContext.Message);
                _log.Debug(msgFailure);
                throw new InvalidOperationException(msgFailure);
            }
            _log.Debug("Analyzer Anonymous authentication successful.");
            CurrentContext = securityContext;
        }

        public void LogOffUser()
        {
            if (CurrentContext != null)
            {
                _az.LogoffUser(CurrentContext);
                CurrentContext = null;
            }
        }

        private void SetAnalyzerInfo()
        {
            // Example of url: http://servername/analyzer/services/analyzer2005.asmx
            string[] urlSplit = _az.Url.Split("/".ToCharArray());
            _analyzerServer = urlSplit[2];
            _analyzerVirtualDirectory = urlSplit[3];
        }

        private static SecurityContext CurrentContext
        {
            get
            {
                if (HttpContext.Current == null)
                    return _currentContext;
                return HttpContext.Current.Session["SecurityContext"] as SecurityContext;
            }
            set
            {
                if (HttpContext.Current == null)
                    _currentContext = value;
                else
                    HttpContext.Current.Session["SecurityContext"] = value;
            }
        }

        public CatalogItem[] ReportCollection()
        {
            var reportCollection1 = new CatalogItem[0];
            var reportCollection2 = new CatalogItem[0];

            CatalogItem[] myReports = _az.ListChildren(CurrentContext, (int)FolderId.MyReport);
            if (myReports != null && myReports.Length > 0 && myReports[0].Success)
            {
                foreach (CatalogItem report in myReports)
                {
                    report.Name += "*";
                    report.Description = "My Report";
                }

                reportCollection1 = new CatalogItem[myReports.Length];
                Array.Copy(myReports, reportCollection1, myReports.Length);
            }

            CatalogItem[] sharedReports = _az.ListChildren(CurrentContext, (int)FolderId.SharedFolder);
            if (sharedReports != null && sharedReports.Length > 0 && sharedReports[0].Success)
            {
                reportCollection2 = new CatalogItem[sharedReports.Length];
                Array.Copy(sharedReports, reportCollection2, sharedReports.Length);
            }

            var allReportsArray = new CatalogItem[reportCollection1.Length + reportCollection2.Length];
            Array.Copy(reportCollection1, allReportsArray, reportCollection1.Length); // My reports added
            Array.Copy(reportCollection2, 0, allReportsArray, reportCollection1.Length, reportCollection2.Length); // Shared reports added

            return allReportsArray;
        }

        public ReportInstance OpenReport(int reportId, PermissionLevel userPermissions)
        {
            // Configure the toolbar. Create a collection of toolbarbuttons that need to be hidden in the UI
            var buttons = new ToolbarButton[]
                              {
								  ToolbarButton.ManageSubscr,
								  ToolbarButton.Subscribe,
                                  ToolbarButton.Close,
                                  ToolbarButton.BookmarkPanel,
                                  ToolbarButton.Config,
                                  ToolbarButton.CreateBookmark,
                                  ToolbarButton.LanguagePanel,
                                  ToolbarButton.PlayVideo,
                                  ToolbarButton.SendMsg,
                                  ToolbarButton.UpdateBookmark,
                                  ToolbarButton.DataSource,
                                  ToolbarButton.ObjectPanel,
                                  ToolbarButton.SchemaTree
                              };


            bool schemaVisible = false;

            if (userPermissions == PermissionLevel.ReportDesigner)
            {
                // A report designer should be able to see datasource and schema panels and buttons
                schemaVisible = true;

                var buttonsCopy = new ToolbarButton[buttons.Length - 2];
                Array.Copy(buttons, buttonsCopy, buttonsCopy.Length);
                buttons = buttonsCopy;
            }

            return _az.OpenReportWithOptions(CurrentContext, reportId, buttons, schemaVisible, false);
        }

        public void CloseReport(ReportInstance reportInstance, bool saveBeforeClose)
        {
            _az.CloseReport(CurrentContext, reportInstance, saveBeforeClose);
        }

        public Role[] GetRoles()
        {
            Role[] roles = _az.ListRoles(CurrentContext);
            return roles;
        }

        public User[] GetUsers()
        {
            return _az.ListUsers(CurrentContext);
        }

        public Role[] GetUserRoles(string userName)
        {
            return _az.GetUserRoles(CurrentContext, userName);
        }

        public bool DeleteUserRoleMembership(int userId, int roleId)
        {
            var roleMember = new RoleMember { MemberId = userId };

            RoleMember outRoleMember = _az.DeleteRoleMember(CurrentContext, roleId, roleMember);

            // Even if user not member of current role the deleteion returns success.
            return outRoleMember.Success;
        }

        public bool DeleteUser(User user)
        {
            User userDeleted = _az.DeleteUser(CurrentContext, user.Id);
            return userDeleted.Success;
        }

        public bool DeleteReport(int reportId)
        {
            CatalogItem catalogItem = _az.DeleteReport(CurrentContext, reportId);
            return catalogItem.Success;
        }

        public User AddUser(string id)
        {
            User user = _az.CreateUser(CurrentContext, new User { Name = id });
            return user;
        }

        public bool EnsureDataSourceExists()
        {
            bool doCreateDataSource = true;

            if (AnalyzerDataSource != null)
            {
                // DataSource found
                doCreateDataSource = false;
            }

            if (doCreateDataSource)
            {
                // Create data source given in constructor
                _log.Debug("Create Analyzer data source.");
                var dataSource = new DataSource();
                dataSource.ServerName = _olapServer;
                dataSource.InitialCatalog = _olapDatabase;
                dataSource.Name = _olapServer;
                dataSource.SourceType = DataSourceType.SqlAs2005;
                DataSource outDataSource = _az.CreateDataSource(CurrentContext, dataSource);
                if (!outDataSource.Success)
                {
                    // Failed to create data source!!!
                    _log.Debug("Failed to create Analyzer data source!");
                    return false;
                }
            }

            return true;
        }

        public CatalogItem AnalyzerDataSource
        {
            get
            {
                if (_analyzerDataSource == null)
                {
                    _log.Debug("List all Analyzer´s datasources.");
                    DataSource[] availableDataSources = _az.ListDataSources(CurrentContext);

                    if (availableDataSources != null)
                    {
                        _log.DebugFormat("Analyzer´s datasources count: {0}.", availableDataSources.Length);
                        foreach (DataSource dataSource in availableDataSources)
                        {
                            if (dataSource.Success
                            && dataSource.ItemType == CatalogItemType.DataSource
                            && dataSource.Name.Trim().ToUpperInvariant() == _olapServer.ToUpperInvariant())
                            {
                                // DataSource found
                                _analyzerDataSource = dataSource;
                                break;
                            }
                        }
						if (_analyzerDataSource == null)
						{
							_log.Debug("Datasources in Analyzer could not be mapped. Maybe the servername in weg.config is incorrect.");
						}
                    }
                    else
                    {
                        _log.Debug("No datasources found for Analyzer");
                    }
                }
                return _analyzerDataSource;
            }
        }

        public CatalogItem SharedReportsFolder
        {
            get
            {
                if (_sharedReportsFolder == null)
                {
                    CatalogItem catalogItem = _az.FindCatalogItemById(CurrentContext, ShareReportId);

                    if (catalogItem != null && catalogItem.ParentId == rootNodeId)
                    {
                        CatalogItem foundFolder = catalogItem;
                        if (foundFolder.Success && foundFolder.ItemType == CatalogItemType.Folder)
                        {
                            // Shared reports folder found
                            _sharedReportsFolder = foundFolder;
                        }
                    }
                }

                return _sharedReportsFolder;
            }
        }

        public int NewReport(CatalogItem report)
        {
            var buttons = new ToolbarButton[]
                              {
                                  ToolbarButton.Close,
                                  ToolbarButton.BookmarkPanel,
                                  ToolbarButton.Config,
                                  ToolbarButton.CreateBookmark,
                                  ToolbarButton.LanguagePanel,
                                  ToolbarButton.PlayVideo,
                                  ToolbarButton.SendMsg,
                                  ToolbarButton.UpdateBookmark,
                                  ToolbarButton.DataSource
                              };

            _log.Debug("Creating new report.");
            ReportInstance reportInstance = _az.CreateReportWithOptions(CurrentContext, report, AnalyzerDataSource.Id,
                                                                        _olapDatabase, "Teleopti Analytics",
                                                                        new PivotTable(), buttons, true, false);

            int reportId = reportInstance.ReportId;

            //Save report
            _az.SaveReport(CurrentContext, reportInstance);

            // Close report instance, report will be opened directly after creation
            _az.CloseReport(CurrentContext, reportInstance, true);

            return reportId;
        }

        public PermissionLevel GetUserPermissions(string userName)
        {
            PermissionLevel userPermissionsOut = PermissionLevel.None;
            PermissionLevel userPermissionsDataSource = PermissionLevel.None;
            CatalogItem dataSource = AnalyzerDataSource;

            // Get user permissions for current datasource object
            _log.DebugFormat("Getting user permissions on Analyzer datasource '{0}' for user '{1}'.", dataSource.Name.Trim(), userName);
            PermissionInfo permissionDataSource = _az.GetUserPermission(CurrentContext, userName, dataSource);

            bool canRead = permissionDataSource.Read == PermissionState.Grant;        // List
            bool canExecute = permissionDataSource.Execute == PermissionState.Grant;  // Execute
            bool canExport = permissionDataSource.Export == PermissionState.Grant;    // Export
            bool canWrite = permissionDataSource.Write == PermissionState.Grant;      // Config

            if (canRead && canExecute && canExport)
            {
                userPermissionsDataSource = canWrite ? PermissionLevel.ReportDesigner : PermissionLevel.GeneralUser;
            }

            if (userPermissionsDataSource != PermissionLevel.None)
            {
                // Get user permissions for current "Shared Reports" folder
                _log.Debug("Getting 'Shared Reports' catalog.");
                CatalogItem catalogItem = _az.FindCatalogItemById(CurrentContext, ShareReportId);

                if (catalogItem != null)
                {
                    if (catalogItem.ParentId == rootNodeId)
                    {
                        CatalogItem sharedReportsFolder = catalogItem;
                        _log.DebugFormat("Getting user permissions on 'Shared Reports' for user '{0}'.", userName);
                        PermissionInfo permissionSharedReports = _az.GetUserPermission(CurrentContext, userName, sharedReportsFolder);

                        canRead = permissionSharedReports.Read == PermissionState.Grant;
                        canExecute = permissionSharedReports.Execute == PermissionState.Grant;
                        canExport = permissionSharedReports.Export == PermissionState.Grant;
                        canWrite = permissionSharedReports.Write == PermissionState.Grant;

                        if (userPermissionsDataSource == PermissionLevel.GeneralUser && canRead && canExecute && canExport)
                        {
                            userPermissionsOut = PermissionLevel.GeneralUser;
                        }
                        else if (userPermissionsDataSource == PermissionLevel.ReportDesigner && canRead && canExecute && canExport && canWrite)
                        {
                            userPermissionsOut = PermissionLevel.ReportDesigner;
                        }
                    }
                    else
                    {
                        userPermissionsOut = PermissionLevel.None;
                    }
                }
                else
                {
                    userPermissionsOut = PermissionLevel.None;
                }
            }

            _log.DebugFormat("User permission for user '{0}' is '{1}'.", userName, userPermissionsOut);

            return userPermissionsOut;
        }

        public bool AssignRoleMembership(int userId, int roleId)
        {
            var roleMember = new RoleMember();
            roleMember.MemberId = userId;
            roleMember.MemberType = RoleMemberType.User;

            RoleMember outRoleMember = _az.AddRoleMember(CurrentContext, roleId, roleMember);
            return outRoleMember.Success;
        }

        public bool SetDefaultRolePermission(int generalUserRoleId, int reportDesignerRoleId)
        {

            CatalogItem catalogItem = _az.FindCatalogItemById(CurrentContext, rootNodeId);
            if (catalogItem != null && (catalogItem.ParentId == 0) && catalogItem.Success)
            {

                // Set permissions on the root node in General User role
                var permissionGeneralUser = new ObjectPermission();
                permissionGeneralUser.MemberType = SecurityMemberType.Role;
                permissionGeneralUser.MemberId = generalUserRoleId;
                permissionGeneralUser.ObjectId = rootNodeId;
                permissionGeneralUser.Permission = (int)PermissionLevel.GeneralUser;


                // Set permissions on the root node in Report Designer role
                var permissionReportDesigner = new ObjectPermission();
                permissionReportDesigner.MemberType = SecurityMemberType.Role;
                permissionReportDesigner.MemberId = reportDesignerRoleId;
                permissionReportDesigner.ObjectId = rootNodeId;
                permissionReportDesigner.Permission = (int)PermissionLevel.ReportDesigner;

                // First remove... 
                BaseState baseState1 = _az.ClearObjectPermissions(CurrentContext, rootNodeId);
                if (baseState1.Success)
                {
                    // ...and then add permissions for root node on General User and Report Designer role.
                    BaseState baseState2 = _az.AddObjectPermission(CurrentContext, permissionGeneralUser);
                    BaseState baseState3 = _az.AddObjectPermission(CurrentContext, permissionReportDesigner);

                    if (!baseState2.Success | !baseState3.Success)
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public Uri GetReportUrl(ReportInstance reportInstance)
        {
            if (reportInstance != null)
            {
	            var serviceUrl = new Uri(_az.Url);

	            string url;
							if (serviceUrl.Scheme == "https")
								url = _az.GetSecureReportInstanceUrl(CurrentContext, reportInstance,
                                                                 string.Format(CultureInfo.InvariantCulture, "{0}/{1}",
                                                                               new object[] { _analyzerServer, _analyzerVirtualDirectory }));
							else
							{
								url = _az.GetReportInstanceUrl(CurrentContext, reportInstance,
																								 string.Format(CultureInfo.InvariantCulture, "{0}/{1}",
																															 new object[] { _analyzerServer, _analyzerVirtualDirectory }));
							}
                Uri reportUri;
                string logText;
                if (url == null)
                    logText = "null!";
				else if (string.IsNullOrEmpty(url))
                    logText = "empty string!";
                else
                    logText = url;
                logText = string.Format(CultureInfo.CurrentCulture, "GetReportInstanceUrl returns URL value: '{0}'", logText);

                try
                {
                    reportUri = new Uri(url);
                }
                catch (UriFormatException uriFormatException)
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "{0} {1}", uriFormatException.Message, logText);
                    logText = string.Format(CultureInfo.CurrentCulture, "{0} [Exception thrown: {1}]", logText, uriFormatException.Message);
                    _log.ErrorFormat(CultureInfo.CurrentCulture, logText);
                    throw new UriFormatException(msg);
                }
                finally
                {
                    _log.DebugFormat(CultureInfo.CurrentCulture, logText);
                }

                return reportUri;
            }
            return null;
        }

        public bool DoReportExist(string name)
        {
            CatalogItem[] catalogItems = _az.FindCatalogItemByName(CurrentContext, name);
            if (catalogItems != null && catalogItems.Length > 0)
            {
                if (catalogItems[0].ItemType == CatalogItemType.Report && catalogItems[0].Success)
                {
                    _log.DebugFormat("Analyzer report with the name '{0}' already exists.", name);
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _az.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
