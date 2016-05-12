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
			controller: ['$scope', 'MoveActivityValidator', '$element', '$attrs', function ($scope, validator, $element, $attrs) {
				var vm = this;
				vm.showMeridian = vm.showMeridian();
				vm.allowNextDay = vm.allowNextDay();
				vm.minuteStep = vm.minuteStep();

				vm.init = init;

				function init(ngModel) {
					setTabNavigation();

					$scope.$watch(function () {
						return vm.newStartTime;
					}, function (newVal, oldVal) {
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
						var newStart = vm.newStartTime.nextDay ? moment(value).add(1, 'days') : moment(value);
						validity = validator.validateMoveToTime(newStart);
						return validity;
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
			controller: ['ActivityService', 'guidgenerator', 'CommandCommon', 'PersonSelection', 'teamScheduleNotificationService','MoveActivityValidator', moveActivityPanelCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function moveActivityPanelCtrl(ActivityService, guidgenerator, commandCommon, personSelectionSvc, NotificationSvc, validator) {
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
		vm.invalidPeople = function () {
			var people = validator.getInvalidPeople().join(', ');
			return people;
		};

		vm.applyMoveActivity = applyMoveActivity;

		function applyMoveActivity() {
			var selectedPersonProjections = personSelectionSvc.getSelectedPersonInfoList();
			var trackId = guidgenerator.newGuid();
			var personActivities = [];
			angular.forEach(selectedPersonProjections, function (personProjection) {
				personActivities.push({
					PersonId: personProjection.personId,
					ShiftLayerIds: personProjection.selectedActivities
				});
			});
			var newStart = vm.newStartTime.nextDay ? moment(vm.newStartTime.startTime).add(1, 'd') : moment(vm.newStartTime.startTime);
			var moveActivityForm = {
				Date: vm.selectedDate(),
				PersonActivities: personActivities,
				StartTime:newStart.format("YYYY-MM-DD HH:mm"),
				TrackedCommandInfo: { TrackId: trackId }
			}
			ActivityService.moveActivity( moveActivityForm ).then(function(response) {
			    if (vm.actionsAfterActivityApply) {
					vm.actionsAfterActivityApply({
						trackId: trackId,
						personIds: personActivities.map(function (agent) { return agent.PersonId; })
					});
				}
				var total = personActivities.length;
				var fail = response.data.length;
				if (fail === 0) {
					NotificationSvc.notify('success', 'SuccessfulMessageForMovingActivity'); 
				} else {
					var description = NotificationSvc.notify('warning', 'PartialSuccessMessageForMovingActivity', [total, total - fail, fail]);
				}
			});
		}
	}
})();
