describe('<remove-day-off>',
	function () {
		'use strict';
		var $rootScope,
			$compile,
			$document,
			$q,
			$httpBackend,
			fakeDayOffService,
			fakePersonSelectionService;

		beforeEach(module('wfm.templates', 'wfm.teamSchedule'));

		beforeEach(module(function ($provide) {
			$provide.service('DayOffService',
				function () {
					fakeDayOffService = new FakeDayOffService();
					return fakeDayOffService;
				});
			$provide.service('PersonSelection', function () {
				fakePersonSelectionService = new FakePersonSelectionService();
				return fakePersonSelectionService;
			});
			
		}));

		beforeEach(inject(function (_$rootScope_, _$compile_, _$document_, _$q_, _$httpBackend_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			$document = _$document_;
			$q = _$q_;
			$httpBackend = _$httpBackend_;
			$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
		}));

		afterEach(function () {
			$document[0].querySelector(".modal-box")
				&& $document[0].querySelector(".modal-box").remove();
		});

		it("should show confirm dialog", function () {
			var dialog = setUp("2018-01-12").dialog;

			var title = dialog.querySelector('.modal-inner-content h3');
			var button = dialog.querySelectorAll("button");

			expect(title.innerHTML.trim()).toEqual("RemoveDayOff");
			expect(button.length).toEqual(2);
			expect(button[0].innerHTML).toEqual("Cancel");
			expect(button[1].innerHTML).toEqual("Apply");
		});
		it("should disapper the dialog if click cancel button", function () {
			var dialog = setUp("2018-01-12").dialog;
			var cancelButton = dialog.querySelectorAll("button")[0];
			cancelButton.click();

			dialog = $document[0].querySelector(".modal-box");
			expect(dialog).toEqual(null);
		});
		it("should call remove day off when click apply button", function () {
			var date = "2018-01-12";
			var document = setUp(date);
			fakePersonSelectionService.setFakeCheckedPersonInfoList();

			var dialog = document.dialog;
			var ctrl = document.removeElement.isolateScope().$ctrl;

			var applyButton = dialog.querySelectorAll("button")[1];
			applyButton.click();

			var removeDayOffData = fakeDayOffService.lastPostData;
			expect(moment(removeDayOffData.Date).format("YYYY-MM-DD")).toEqual(date);
			expect(removeDayOffData.PersonIds).toEqual(personList.map(function (p) { return p.PersonId; }));
			expect(removeDayOffData.TrackedCommandInfo.TrackId).toEqual(ctrl.trackId);
		});

		function setUp(inputDate) {
			var date;
			var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
			var scope = $rootScope.$new();
			if (inputDate == null)
				date = moment('2016-06-15').toDate();
			else
				date = new Date(inputDate);

			scope.curDate = date;

			var container = $compile(html)(scope);
			scope.$apply();

			var vm = container.isolateScope().vm;
			vm.setReady(true);
			vm.setActiveCmd('RemoveDayOff');
			scope.$apply();

			var popDialog = $document[0].querySelector(".modal-box");
			var removeElement = angular.element(container[0].querySelector("remove-day-off"));
			return {
				dialog: popDialog,
				removeElement: removeElement
			}
		}

		function FakeDayOffService() {
			this.lastPostData = null;

			this.getAllDayOffTemplates = function () {
				return $q(function (resolve, reject) {
					resolve([
						{ Id: 'template1', Name: 'template1' },
						{ Id: 'template2', Name: 'template2' }
					]);
				});
			}
			this.addDayOff = function (input) {
				this.lastPostData = input;
			}
			this.removeDayOff = function (input) {
				this.lastPostData = input;
			}
		}

		var personList = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: null,
				ScheduleEndTime: null
			}];
		function FakePersonSelectionService() {
			var checkedPersonList = [];

			this.setFakeCheckedPersonInfoList = function (input) {
				checkedPersonList = input || personList;
			}

			this.getCheckedPersonInfoList = function () {
				return checkedPersonList;
			}


		}

	});