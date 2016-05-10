(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.directive('moveActivityPanel', moveActivityPanel)
		.directive('moveActivityInput', moveActivityInput);

	function moveActivityInput() {
		return {
			restrict: 'E',
			require: ['ngModel', 'moveActivityInput'],
			scope: {
				newStartTime: '=ngModel',
				showMeridian: '&',
				minuteStep: '&',
				allowNextDay: '&'
			},
			bindToController: true,
			controller: ['$scope', 'PersonSelection', '$element', '$attrs', function ($scope, personSelectionSvc, $element, $attrs) {
				var vm = this;
				vm.showMeridian = vm.showMeridian();
				vm.allowNextDay = vm.allowNextDay();
				vm.minuteStep = vm.minuteStep();

				vm.init = init;

				function init(ngModel) {
					var selectedAgents;

					setTabNavigation();

					$scope.$watch(function () {
						return vm.newStartTime;
					}, function (newVal, oldVal) {
						selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
						var validity = validateStartTime(newVal.startTime);
						ngModel.$setValidity('startTime', validity);
					}, true);

					function addFocusListener(element) {
						angular.element(element).on('focus', function (event) {
							event.target.select();
						});
					}

					function setTabNavigation() {
						if (angular.isDefined($attrs.tabindex)) {
							var timepickerInputs = $element[0].querySelectorAll('input[type=text]');
							for (var i = 0; i < timepickerInputs.length; i++) {
								timepickerInputs[i].tabIndex = $attrs.tabindex;
								addFocusListener(timepickerInputs[i]);
							}
						}

						$element.on('focus', function (event) {
							event.target.querySelector('input').focus();
						});
					}

					function validateStartTime(value) {
						var validity = true;
						selectedAgents.forEach(function (agent) {
							if (!agent.scheduleEndTime) {
								return;
							}
							var formattedStartTime = moment(value).format('YYYY-MM-DD HH:mm');
							validity = !vm.newStartTime.nextDay || sameDayAndNoLaterThan(formattedStartTime, agent.scheduleEndTime);
						});
						return validity;
					}

					function sameDayAndNoLaterThan(dateStr1, dateStr2) {
						var moment1 = moment(dateStr1);
						var moment2 = moment(dateStr2);
						return moment1.isSame(moment2, 'day') && moment2.isAfter(moment1);
					}
				}
			}],
			link: function (scope, elem, attr, ctrls) {
				var ngModel = ctrls[0];
				var moveActivityInputCtrl = ctrls[1];
				moveActivityInputCtrl.init(ngModel);
			},
			controllerAs: 'vm',
			template: [
				'<uib-timepicker show-meridian="vm.showMeridian" minute-step="vm.minuteStep" ng-model="vm.newStartTime.startTime"></uib-timepicker>',
				'<div class="wfm-switch">',
					'<input type="checkbox" id="mvActNextDay" ng-model="vm.newStartTime.nextDay" ng-disabled="!vm.allowNextDay" />',
					'<label for="mvActNextDay">',
						'<span class="wfm-switch-label">{{"NextDay" | translate}}</span>',
						'<span class="wfm-switch-toggle"></span>',
					'</label>',
				'</div>'
			].join('\n')
		};
	}

	function moveActivityPanel() {
		return {
			restrict: 'E',
			scope: {
				tabindex: '@tabindex',
				selectedDate: '&',
				defaultStart: '&?',
				actionsAfterActivityApply: '&?'
			},
			templateUrl: 'js/teamSchedule/html/moveActivity.html',
			link: function (scope, iElement, iAttrs, controller) {
				iElement.attr('tabIndex', '-1');
			},
			controller: ['$scope', '$element', moveActivityPanelCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function moveActivityPanelCtrl($scope, $element) {
		var vm = this;
		var ADD_HOURS = 1;
		var MINUTE_STEP = 5;
		vm.showMeridian = false;
		vm.allowNextDay = true;
		vm.minuteStep = MINUTE_STEP;

		vm.defaultStartTime = moment(vm.defaultStart()).add(ADD_HOURS, 'hour').toDate();
		vm.newStartTime = {
			startTime: vm.defaultStartTime,
			nextDay: false
		};

		vm.applyMoveActivity = applyMoveActivity;

		function applyMoveActivity() {
		}
	}
})();
