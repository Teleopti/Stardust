(function () {
	'use strict';

	angular.module('wfm.requests').run([
        '$templateCache', function ($templateCache) {

        	var template =
						'<div>' +

						
						'					<md-tooltip class=\"account-tooltip\">' +
						'							<div ng-repeat=\"accountDetail in row.entity[\'PersonAccountSummary\'].PersonAccountSummaryDetails\">'+
						'							{{::accountDetail.StartDate  | date : \"shortDate"}} : ' +
						'							{{::accountDetail.RemainingDescription}}' +
						'							{{::accountDetail.TrackingTypeDescription}} {{ \"Remaining\" | translate}}' +
						'							</div>'+
						'					</md-tooltip>' +



						'	<div class=\"absence-account-cell\" ng-repeat=\"accountDetail in row.entity[\'PersonAccountSummary\'].PersonAccountSummaryDetails\">' +
						'		<div class=\"absence-account-cell-content\">' +

				

						'					<span class=\"absence-account-date arrow_box\" ng-if=\"!$first\">' +
						'						{{::accountDetail.StartDate  | date : \"MMM-dd\"}}' +
						'					</span>' +
						//'					<span class=\"account-seperator\" ng-if=\"!$first\">' +
						'					</span>' +
						'					<span>'+
						'						{{::accountDetail.RemainingDescription}}' +
					
						'					</span>' +
						//'					<span ng-if=\"$last\">' +
						//'						({{::accountDetail.TrackingTypeDescription}})' +
						//'					</span>' +
						
        			    '		</div>' +
						'	</div>' +
						
						'<div>';

        	$templateCache.put("requests-absence-person-account-overview.html", template);
        }
	]);
})();
