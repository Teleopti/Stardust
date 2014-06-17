using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Matrix
{
    public interface IMatrixNavigationModel
    {
        IEnumerable<IMatrixFunctionGroup> GroupedPermittedMatrixFunctions { get; }
        IEnumerable<IApplicationFunction> OrphanPermittedMatrixFunctions { get; }
        AuthenticationTypeOption AuthenticationType { get; }
        Uri MatrixWebsiteUrl { get; }
        Guid? BusinessUnitId { get; }
        IEnumerable<IApplicationFunction> PermittedOnlineReportFunctions { get; }
        IEnumerable<IApplicationFunction> PermittedMatrixFunctions { get; }
    }

    public class MatrixNavigationModel : IMatrixNavigationModel
    {
        private readonly Func<string> _matrixWebsiteUrlGetter;
        private IEnumerable<IMatrixFunctionGroup> _matrixFunctionGroups;

        public MatrixNavigationModel() : this(() => StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["MatrixWebSiteUrl"])
        {
        }

        public MatrixNavigationModel(Func<string> matrixWebsiteUrlGetter)
        {
            _matrixWebsiteUrlGetter = matrixWebsiteUrlGetter;
        }

        public IEnumerable<IApplicationFunction> PermittedMatrixFunctions
        {
			get
			{
				return PrincipalAuthorization.Instance()
					.GrantedFunctionsBySpecification(
						new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix)
					);
			}
        }

        public IEnumerable<IApplicationFunction> PermittedOnlineReportFunctions
        {
            get
            {
            	IEnumerable<IApplicationFunction> onlineReportFunctions =
            		PrincipalAuthorization.Instance()
            			.GrantedFunctionsBySpecification(
            				new IsOnlineReportFunctionSpecification()
            			);

                return onlineReportFunctions;
            }
        }

        private class IsOnlineReportFunctionSpecification : Specification<IApplicationFunction>
        {
            private readonly string[] _onlineReportForeignIdList;

            public IsOnlineReportFunctionSpecification()
            {
                _onlineReportForeignIdList = new[] {"0055", "0059", "0064"};
            }


            public override bool IsSatisfiedBy(IApplicationFunction obj)
            {
                return _onlineReportForeignIdList.Contains(obj.ForeignId);
            }
        }

        private static IEnumerable<string> analysisReports()
        {
            // old "21", "18", "17", "19", "26", "29"
	        return new[]
	        {
		        "132E3AF2-3557-4EA7-813E-05CD4869D5DB",
		        "63243F7F-016E-41D1-9432-0787D26F9ED5",
		        "009BCDD2-3561-4B59-A719-142CD9216727",
		        "BAA446C2-C060-4F39-83EA-B836B1669331",
		        "2F222F0A-4571-4462-8FBE-0C747035994A",
		        "C052796F-1C8A-4905-9246-FF1FF8BD30E5"
	        };
        }

        private static IEnumerable<string> preferencesReports()
        {
            // old "2", "1" 
            return new[]{
                "0E3F340F-C05D-4A98-AD23-A019607745C9",
                "5C133E8F-DF3E-48FC-BDEF-C6586B009481"};
        }

        private static IEnumerable<string> EmployeeReports()
        {
            // old "4", "20", "22","23", "25", "27"
            return new[]{
                "C5B88862-F7BE-431B-A63F-3DD5FF8ACE54",
                "35649814-4DE8-4CB3-A51C-DDBA2A073E09",
                "D45A8874-57E1-4EB9-826D-E216A4CBC45B",
                "EB977F5B-86C6-4D98-BEDF-B79DC562987B",
                "E15400E7-892A-4EDE-9377-AE693AA56829",
                "8DE1AB0F-32C2-4619-A2B2-97385BE4C49C",
				"A56B3EEF-17A2-4778-AA8A-D166232073D2"};
        }

        private static IEnumerable<string> agentReports()
        {
            // old "15", "13", "12", "16", "11", "24" 
            return new[]{
                "71BDB56D-C12F-489B-8275-04873A668D90",
                "0065AA84-FD47-4022-ABE3-DD1B54FD096C",
                "D1ADE4AC-284C-4925-AEDD-A193676DBD2F",
				"6A3EB69B-690E-4605-B80E-46D5710B28AF",
                "4F5DDE81-C264-4756-B1F1-F65BFE54B16B",
                "80D31D84-68DB-45A7-977F-75C3250BB37C",
                "047B138C-DE3A-426A-99B0-00C5BA826AF2",
                "479809D8-4DAE-4852-BF67-C98C3744918D"
                };
        }

        private static IEnumerable<string> forecastReports()
        {
            // old "7", "10" 
            return new[]{
                "720A5D88-D2B5-49E1-83EE-8D05239094BF",
                "8D8544E4-6B24-4C1C-8083-CBE7522DD0E0"};
        }

        private static IEnumerable<string> serviceLevelReports()
        {
            // old "8", "9", "14" 
            return new[]{
                "C232D751-AEC5-4FD7-A274-7C56B99E8DEC",
                "AE758403-C16B-40B0-B6B2-E8F6043B6E04",
                "F7937D02-FA54-4679-AF70-D9798E1690D5"};
        }

        public IEnumerable<IMatrixFunctionGroup> GroupedPermittedMatrixFunctions
        {
            get
            {
                if (_matrixFunctionGroups == null)
                {
                    _matrixFunctionGroups =
                        new List<IMatrixFunctionGroup>
                            {
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.ScheduleAnalysis,
                                        ApplicationFunctions =
                                            (from a in PermittedMatrixFunctions
                                             where analysisReports().Contains(a.ForeignId)
                                             select a).ToList()
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.Preferences,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where preferencesReports().Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.EmployeeInformation,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where EmployeeReports().Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.AgentPerformance,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where agentReports().Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.ForecastingPerformance,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where forecastReports().Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.ServiceLevelAnalysis,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where serviceLevelReports().Contains(a.ForeignId)
                                            select a
                                    },
                                
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.Improve,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where new[] {"7F918C26-4044-4F6B-B0AE-7D27625D052E"}.Contains(a.ForeignId)
                                            select a
                                    }
                            };
                    _matrixFunctionGroups = from g in _matrixFunctionGroups where g.ApplicationFunctions.Any() select g;
                }
                return _matrixFunctionGroups;
            }
        }

        public IEnumerable<IApplicationFunction> OrphanPermittedMatrixFunctions
        {
            get
            {
					var groupedMatrixFunctionForeignIds =
						(from g in GroupedPermittedMatrixFunctions
						from f in g.ApplicationFunctions
						select f.ForeignId).ToList();
                return from f in PermittedMatrixFunctions
                       where groupedMatrixFunctionForeignIds.Contains(f.ForeignId) == false
                       select f;
            }
        }

        public AuthenticationTypeOption AuthenticationType
        {
            get { return StateHolderReader.Instance.StateReader.SessionScopeData.AuthenticationTypeOption; }
        }

        public Uri MatrixWebsiteUrl
        {
            get
            {
                return new Uri(_matrixWebsiteUrlGetter.Invoke() + "?ReportID={0}&forceformslogin={1}&buid={2}");
            }
        }

        public Guid? BusinessUnitId
        {
            get { return ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id; }
        }

        public string DataSourceName
        {
            get { return ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).DataSource.DataSourceName; }
        }
    }

    public interface IMatrixFunctionGroup
    {
        string LocalizedDescription { get; set; }
        IEnumerable<IApplicationFunction> ApplicationFunctions { get; set; }
    }

    public class MatrixFunctionGroup : IMatrixFunctionGroup
    {
        public string LocalizedDescription { get; set; }
        public IEnumerable<IApplicationFunction> ApplicationFunctions { get; set; }
    }
}
