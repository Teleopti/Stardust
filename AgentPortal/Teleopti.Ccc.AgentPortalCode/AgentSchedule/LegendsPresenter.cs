using System;
using System.Collections;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    public class LegendsPresenter
    {
        private readonly ILegendsView _view;
        private int _noRowsInAbsence = 1;
        private int _noRowsInActivity = 1;
        private readonly ITeleoptiSchedulingService _sdk;

        public LegendsPresenter(ILegendsView view, ITeleoptiSchedulingService sdk)
        {
            _sdk = sdk;
            _view = view;
        }

        public void Initialize()
        {
            _view.AbsenceDataSource = createAbsenceDatasource();
            _view.ActivityDataSource = createActivityDatasource();
        }

        private ArrayList createActivityDatasource()
        {
            var arrActivityDto = _sdk.GetActivities(new LoadOptionDto { LoadDeleted = false, LoadDeletedSpecified = true });       
            var datasource = new ArrayList();
            Array.ForEach(arrActivityDto, a => datasource.Add(new LegendModel(a)));
            _noRowsInActivity = datasource.Count;
            return datasource;
        }

        private ArrayList createAbsenceDatasource()
        {
            var arrAbsenceDto = _sdk.GetAbsences(new AbsenceLoadOptionDto { LoadDeleted = false, LoadDeletedSpecified = true });
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