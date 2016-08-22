using System;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IActivityChangeChecker
	{
		void CheckForActivityChanges();
	}

	public class ActivityChangeChecker
	{
		private readonly ILicenseActivatorProvider _provider;
		private readonly LicensedActivityChangeChecker _licensedChecker;
		private readonly UnlicensedActivityChangeChecker _unlicensedChecker;
		private readonly Func<ILicenseActivatorProvider, LicensedActivityChangeChecker, UnlicensedActivityChangeChecker, IActivityChangeChecker> _checker;

		public ActivityChangeChecker(ILicenseActivatorProvider provider,
			LicensedActivityChangeChecker licensedChecker, 
			UnlicensedActivityChangeChecker unlicensedChecker, 
			Func<ILicenseActivatorProvider, LicensedActivityChangeChecker, UnlicensedActivityChangeChecker, IActivityChangeChecker> checker)
		{
			_provider = provider;
			_licensedChecker = licensedChecker;
			_unlicensedChecker = unlicensedChecker;
			_checker = checker;
		}

		public void CheckForActivityChanges()
		{
			_checker(_provider, _licensedChecker, _unlicensedChecker).CheckForActivityChanges();
		}
	}

	public class LicensedActivityChangeChecker : IActivityChangeChecker
	{
		private readonly IContextLoader _contextLoader;
		private readonly RtaProcessor _processor;

		public LicensedActivityChangeChecker(
			IContextLoader contextLoader,
			RtaProcessor processor
			)
		{
			_contextLoader = contextLoader;
			_processor = processor;
		}

		public void CheckForActivityChanges()
		{
			_contextLoader.ForAll(person =>
			{
				_processor.Process(person);
			});
		}
	}

	public class UnlicensedActivityChangeChecker : IActivityChangeChecker
	{
		public void CheckForActivityChanges()
		{
		}
	}
}