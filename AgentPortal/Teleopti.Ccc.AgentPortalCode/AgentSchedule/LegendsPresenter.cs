using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
	public interface ILegendLoader
	{
		ICollection<ActivityDto> GetActivities();
		ICollection<AbsenceDto> GetAbsences();
	}

	public class LegendLoader : ILegendLoader
	{
		private readonly Func<ITeleoptiSchedulingService> _sdk;
		private DateTime _lastLoad = DateTime.MinValue;
		private ICollection<ActivityDto> _activities;
		private ICollection<AbsenceDto> _absences;
		private object Lock = new object();

		public LegendLoader(Func<ITeleoptiSchedulingService> sdk)
		{
			_sdk = sdk;
		}

		public ICollection<ActivityDto> GetActivities()
		{
			lock (Lock)
			{
				if (_activities == null || _lastLoad < DateTime.UtcNow.AddMinutes(-10))
				{
					_activities = _sdk.Invoke().GetActivities(new LoadOptionDto { LoadDeleted = false });
					_lastLoad = DateTime.UtcNow;
				}
				return _activities;
			}
		}

		public ICollection<AbsenceDto> GetAbsences()
		{
			lock (Lock)
			{
				if (_absences == null || _lastLoad < DateTime.UtcNow.AddMinutes(-10))
				{
					_absences = _sdk.Invoke().GetAbsences(new AbsenceLoadOptionDto { LoadDeleted = false, LoadRequestable = false});
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
            foreach (var activityDto in arrActivityDto)
            {
                datasource.Add(new LegendModel(activityDto));
            }
            _noRowsInActivity = datasource.Count;
            return datasource;
        }

        private ArrayList createAbsenceDatasource()
        {
            var arrAbsenceDto = _legendLoader.GetAbsences();
            var datasource = new ArrayList();
            foreach (var absenceDto in arrAbsenceDto)
            {
                datasource.Add(new LegendModel(absenceDto));
            }
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