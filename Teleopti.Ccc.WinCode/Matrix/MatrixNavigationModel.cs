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
            get { return TeleoptiPrincipal.Current.PrincipalAuthorization.GrantedFunctionsBySpecification(new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix)); }
        }

        public IEnumerable<IApplicationFunction> PermittedOnlineReportFunctions
        {
            get
            {
                IEnumerable<IApplicationFunction> onlineReportFunctions =
                    TeleoptiPrincipal.Current.PrincipalAuthorization.GrantedFunctionsBySpecification(
                        new IsOnlineReportFunctionSpecification());

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
                                             where new[] {"21", "18", "17", "19", "26"}.Contains(a.ForeignId)
                                             select a).ToList()
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.Preferences,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where new[] {"2", "1"}.Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.EmployeeInformation,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where new[] {"4", "20", "22","23", "25", "27"}.Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.AgentPerformance,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where new[] {"15", "13", "12", "16", "11", "24"}.Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.ForecastingPerformance,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where new[] {"10", "7"}.Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.ServiceLevelAnalysis,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where new[] {"8", "14", "9"}.Contains(a.ForeignId)
                                            select a
                                    },
                                new MatrixFunctionGroup
                                    {
                                        LocalizedDescription = Resources.Improve,
                                        ApplicationFunctions =
                                            from a in PermittedMatrixFunctions
                                            where new[] {"3"}.Contains(a.ForeignId)
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
                    from g in GroupedPermittedMatrixFunctions
                    from f in g.ApplicationFunctions
                    select f.ForeignId;
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
            get { return ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id; }
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
