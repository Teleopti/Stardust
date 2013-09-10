using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class AuthorizationServiceWithFullAccess : IAuthorizationService
	{

		public void SynchronizeMatrixReportFunctions()
		{
			throw new NotImplementedException();
		}

		public void LoadPermissions(IPerson person, IAuthorizationSettings settings)
		{
			//
		}

		public bool IsPermitted(IApplicationFunction applicationFunction)
		{
			throw new NotImplementedException();
		}

		public bool HasPermission(IApplicationFunction applicationFunction)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Refreshes the rights.
		/// </summary>
		/// <remarks>
		/// Created by: tamasb
		/// Created date: 11/28/2007
		/// </remarks>
		public void RefreshPermissions()
		{
			throw new NotImplementedException();
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IApplicationFunction> PermittedApplicationFunctions
		{
			get { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IApplicationFunction> PermittedApplicationModules
		{
			get { throw new NotImplementedException(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IApplicationFunction> LicensedApplicationModules
		{
			get { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IApplicationFunction> PermittedMatrixFunctions
		{
			get
			{
				ApplicationFunction applicationFunction1 = new ApplicationFunction("ApplicationFunction1");
				applicationFunction1.ForeignId = "1";

				ApplicationFunction applicationFunction2 = new ApplicationFunction("ApplicationFunction2");
				applicationFunction2.ForeignId = "2";

				IList<IApplicationFunction> list = new List<IApplicationFunction> { applicationFunction1, applicationFunction2 };                    
				return list;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IApplicationFunction> AllApplicationModules
		{
			get { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IApplicationFunction> AvailableApplicationFunctions
		{
			get { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IApplicationFunction> AvailableApplicationModules
		{
			get { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IList<IApplicationFunction> AvailableMatrixFunctions
		{
			get { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IAvailableData AvailableData
		{
			get { throw new NotImplementedException(); }
		}

		public IList<DateTimePeriod> AvailablePersonPeriods(IPerson person, DateTimePeriod period)
		{
			return new List<DateTimePeriod> { period };
			//return new List<DateTimePeriod>() { new DateTimePeriod(1600, 1, 1, 2030, 1, 1) };
		}

		public IList<DateTimePeriod> PermittedPeriods(IPerson person, IApplicationFunction applicationFunction, DateTimePeriod period)
		{
			return new List<DateTimePeriod> { period };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "person"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "period"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public IList<DateTimePeriod> PermittedPeriods(IPerson person, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IAvailableData AggregatedPermittedData(IApplicationFunction applicationFunction)
		{
			return new AvailableData();
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IAuthorizationSettings CurrentAuthorizationSettings
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IPerson CurrentPerson
		{
			get { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IDictionary<string, IAuthorizationStep> AuthorizationStepCollection
		{
			get { throw new NotImplementedException(); }
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public IList<T> FindAuthorizationStepProvidedList<T>(string key) where T : IAuthorizationEntity
		{
			throw new NotImplementedException();
		}

		public bool IsPermitted(string applicationFunctionPath)
		{
			return true;
		}

		public bool IsPermitted(ITeam team, IApplicationFunction applicationFunction)
		{
			return true;
		}

		public bool IsPermitted(IPerson person, IApplicationFunction applicationFunction, DateTime queryDate)
		{
			return true;
		}

		public bool IsPermitted(ISite site, IApplicationFunction applicationFunction)
		{
			return true;
		}

		public bool IsPermitted(IBusinessUnit businessUnit, IApplicationFunction applicationFunction)
		{
			return true;
		}

		public bool IsPermitted(IScheduleParameters scheduleParameters, IApplicationFunction applicationFunction)
		{
			return true;
		}

		public bool IsPermitted(IScheduleParameters scheduleParameters, string applicationFunctionPath)
		{
			return true;
		}

		public bool IsPermitted(IPersistableScheduleData persistableScheduleData)
		{
			return true;
		}

		public MultipleBool IsPermitted<T>(IEnumerable<T> selectedScheduleParts, IApplicationFunction applicationFunction) where T : IScheduleParameters
		{
			return MultipleBool.AllTrue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IPerson LoadedPerson
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets the list of loaded application functions.
		/// </summary>
		/// <value>The application functions.</value>
		public IList<IApplicationFunction> LicensedApplicationFunctions
		{
			get
			{
				return new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList.ToList();
			}
		}

		public void StartBunchQueries()
		{
			throw new NotImplementedException();
		}

		public void EndBunchQueries()
		{
			throw new NotImplementedException();
		}

		public void LoadPermissions(IPerson person, IAuthorizationSettings settings, IUnitOfWork unitOfWork)
		{
		}

		public IEnumerable<DateOnlyPeriod> PermittedPeriods(IPerson person, IApplicationFunction application, DateOnlyPeriod period)
		{
			return new List<DateOnlyPeriod> { period };
		}

		public IList<IApplicationFunction> GrantedApplicationFunctions
		{
			get { return null; }
		}

		public IList<IApplicationFunction> GrantedApplicationModules
		{
			get { return null; }
		}

		public IList<IApplicationFunction> AllApplicationFunctions
		{
			get { return null; }
		}
	}


}
