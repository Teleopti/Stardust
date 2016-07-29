﻿(function () {
	'use strict';

	angular.module('wfm.requests').run([
        '$templateCache', function ($templateCache) {

        	var template =
						'<div>' +
						'		<md-tooltip class=\"account-tooltip\">' +
						'				<div ng-repeat=\"accountDetail in row.entity[\'PersonAccountSummaryViewModel\'].PersonAccountSummaryDetails\">' +
						'				<div ng-if=\"$first\"><b>{{\"PersonAccount\" | translate}} {{\"Remaining\" | translate}}</b></div>	' +
						'				{{::accountDetail.StartDate  | date : \"shortDate"}} : ' +
						'				{{::accountDetail.RemainingDescription}}' +
						'				{{::accountDetail.TrackingTypeDescription}}' +
						'				</div>' +
						'		</md-tooltip>' +
						'		<div class=\"absence-account-cell\" ng-repeat=\"accountDetail in row.entity[\'PersonAccountSummaryViewModel\'].PersonAccountSummaryDetails | filter:query as personAccountSummaryDetails\">' +
						'			<div class=\"absence-account-cell-content arrow-box\" ng-class=\" personAccountSummaryDetails.length <= $index+1 ? \'absence-account-cell-content arrow-box arrow-box-no-border\' : \'\' \">' +
						'					{{::accountDetail.RemainingDescription}}' +
						'					{{::accountDetail.TrackingTypeDescription}}' +
        			    '			</div>' +
						'		</div>' +
						'<div>';

        	$templateCache.put("requests-absence-person-account-overview.html", template);
        }
	]);
})();
