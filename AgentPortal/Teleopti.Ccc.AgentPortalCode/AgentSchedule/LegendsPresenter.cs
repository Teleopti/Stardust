using System;
using System.Collections;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
	public interface ILegendLoader
	{
		ActivityDto[] GetActivities();
		AbsenceDto[] GetAbsences();
	}

	public class LegendLoader : ILegendLoader
	{
		private readonly Func<TeleoptiSchedulingService> _sdk;
		private DateTime _lastLoad = DateTime.MinValue;
		private ActivityDto[] _activities;
		private AbsenceDto[] _absences;
		private object Lock = new object();

		public LegendLoader(Func<TeleoptiSchedulingService> sdk)
		{
			_sdk = sdk;
		}

		public ActivityDto[] GetActivities()
		{
			lock (Lock)
			{
				if (_activities == null || _lastLoad < DateTime.UtcNow.AddMinutes(-10))
				{
					_activities = _sdk.Invoke().GetActivities(new LoadOptionDto { LoadDeleted = false, LoadDeletedSpecified = true });
					_lastLoad = DateTime.UtcNow;
				}
				return _activities;
			}
		}

		public AbsenceDto[] GetAbsences()
		{
			lock (Lock)
			{
				if (_absences == null || _lastLoad < DateTime.UtcNow.AddMinutes(-10))
				{
					_absences = _sdk.Invoke().GetAbsences(new AbsenceLoadOptionDto { LoadDeleted = false, LoadDeletedSpecified = true,LoadRequestableSpecified = true});
					_lastLoad = DateTime.UtcNow;
				}
				return _absences;
			}
		}
	}

	public class LegendsPresenter
    {
        private readonly ILegendsView _view;
		private readonly ILegendLoader _legendLoader;
		private int _noRowsInAbsence = 1;
        private int _noRowsInActivity = 1;

        public LegendsPresenter(ILegendsView view, ILegendLoader legendLoader)
        {
            _view = view;
	        _legendLoader = legendLoader;
        }

        public void Initialize()
        {
            _view.AbsenceDataSource = createAbsenceDatasource();
            _view.ActivityDataSource = createActivityDatasource();
        }

        private ArrayList createActivityDatasource()
        {
            var arrActivityDto = _legendLoader.GetActivities();
            var datasource = new ArrayList();
            Array.ForEach(arrActivityDto, a => datasource.Add(new LegendModel(a)));
            _noRowsInActivity = datasource.Count;
            return datasource;
        }

        private ArrayList createAbsenceDatasource()
        {
            var arrAbsenceDto = _legendLoader.GetAbsences();
            var datasource = new ArrayList();
            Array.ForEach(arrAbsenceDto, a => datasource.Add(new LegendModel(a)));
            _noRowsInAbsence = datasource.Count;
            return datasource;
        }

        //Stuff to calculate the height, not fun, but 
        public int Height()
        {
            const int maxHeight = 600;
            const int rowHeight = 17;
            const int extra = 30;

            var actHeight = (_noRowsInActivity * rowHeight) + extra;
            
            if (actHeight > maxHeight)
                actHeight = maxHeight;
            
            var absHeight = (_noRowsInAbsence * rowHeight) + extra;

            if (absHeight > maxHeight)
                absHeight = maxHeight;
            
            _view.ActivityHeight = actHeight;
            _view.AbsenceHeight = absHeight;
            return _view.ActivityHeight + _view.AbsenceHeight+8;
        }
    }
}