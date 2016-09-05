﻿(function () {
	'use strict';

	angular.module('ui.bootstrap.timepicker')
		.run([
			'$templateCache', function ($templateCache) {
				$templateCache.put('uib/template/timepicker/timepicker.html',
					'<table class="uib-timepicker">' +
					'  <tbody>' +
					'    <tr>' +
					'      <td class="form-group uib-time hours" ng-class="{\'has-error\': invalidHours}">' +
					'        <input type="text" placeholder="HH" ng-model="hours" ng-change="updateHours()" class="text-center" ng-readonly="::readonlyInput" maxlength="2" tabindex="{{::tabindex}}" ng-disabled="noIncrementHours()" ng-blur="blur()">' +
					'      </td>' +
					'      <td class="uib-separator">:</td>' +
					'      <td class="form-group uib-time minutes" ng-class="{\'has-error\': invalidMinutes}">' +
					'        <input type="text" placeholder="MM" ng-model="minutes" ng-change="updateMinutes()" class="text-center" ng-readonly="::readonlyInput" maxlength="2" tabindex="{{::tabindex}}" ng-disabled="noIncrementMinutes()" ng-blur="blur()">' +
					'      </td>' +
					'      <td ng-show="showSeconds" class="uib-separator">:</td>' +
					'      <td class="form-group uib-time seconds" ng-class="{\'has-error\': invalidSeconds}" ng-show="showSeconds">' +
					'        <input type="text" placeholder="SS" ng-model="seconds" ng-change="updateSeconds()" class="text-center" ng-readonly="readonlyInput" maxlength="2" tabindex="{{::tabindex}}" ng-disabled="noIncrementSeconds()" ng-blur="blur()">' +
					'      </td>' +
					'      <td ng-show="showMeridian" class="uib-time am-pm"><md-button type="button" ng-class="{disabled: noToggleMeridian()}" ng-click="toggleMeridian()" ng-disabled="noToggleMeridian()" tabindex="{{::tabindex}}">{{meridian}}</md-button></td>' +
					'    </tr>' +
					'  </tbody>' +
					'</table>'
				);
			}
		]);
})();
