describe('<shift-editor>',
	function () {
		'use strict';

		var $rootScope,
			$compile,
			$document;

		beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
		beforeEach(inject(function (_$rootScope_, _$compile_, _$document_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			$document = _$document_;
		}));
		beforeEach(function () { moment.locale('sv'); });
		afterEach(function () { moment.locale('en'); });

		it("should render correctly", function () {
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-05-16'
			});
			var element = panel[0];

			expect(element.querySelector('.name').innerText.trim()).toEqual('Agent 1');
			expect(element.querySelector('.date').innerText).toEqual('2018-05-16');
		});

		it('should show underlying info icon if schedule has underlying activities', function () {
			var scheduleDate = "2018-05-16";
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-05-16',
				UnderlyingScheduleSummary: {
					"PersonalActivities": [{
						"Description": "personal activity",
						"Start": scheduleDate + ' 10:00',
						"End": scheduleDate + ' 11:00'
					}]
				},
				HasUnderlyingSchedules: function () { return true; }
			});

			var element = panel[0];
			expect(element.querySelectorAll('.underlying-info').length).toBe(1);
		});

		it('should render time line correctly', function () {
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-03-25 05:00",
						Minutes: 120
					}]
				}]
			}, "2018-05-21", "Europe/Berlin");
			var timeline = panel[0].querySelector(".timeline");
			var intervals = panel[0].querySelectorAll(".timeline .interval");
			var hours = panel[0].querySelectorAll(".timeline .interval .label>span");
			var textHours = [].slice.call(hours).map(function (hourEl) { return hourEl.innerText.trim() });
			expect(!!timeline).toBeTruthy();
			expect(intervals.length).toBe(37);
			expect(textHours).toEqual( ["00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00"]);
			expect(intervals[0].querySelectorAll('.tick').length).toBe(12);
			expect(intervals[0].querySelectorAll('.tick.hour').length).toBe(1);
			expect(intervals[0].querySelectorAll('.tick.half-hour').length).toBe(1);
			expect(intervals[intervals.length - 1].querySelectorAll('.tick').length).toBe(1);
		});

		it('should render time line correctly on DST', function () {
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-03-25',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-03-25 05:00",
						Minutes: 120
					}]
				}]
			}, "2018-03-25", "Europe/Berlin");
			var timeline = panel[0].querySelector(".timeline");
			var intervals = panel[0].querySelectorAll(".timeline .interval");
			var hours = panel[0].querySelectorAll(".timeline .interval .label>span");
			var textHours = [].slice.call(hours).map(function (hourEl) { return hourEl.innerText.trim() });
			expect(!!timeline).toBeTruthy();
			expect(intervals.length).toBe(36);
			expect(textHours).toEqual(["00:00", "01:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00"]);
		});

		it('should show all shifts correctly', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 05:30",
						Minutes: 120
					}, {
						Color: "#FFC080",
						Description: "Sales",
						Start: "2018-05-21 07:30",
						Minutes: 60
					}, {
						Color: "#000000",
						Description: "Email",
						Start: "2018-05-21 08:30",
						Minutes: 30,
						IsOvertime: true,
						UseLighterBorder: true
					},
					{
						Color: "#000000",
						Description: "Email",
						Start: "2018-05-21 09:00",
						Minutes: 30,
						IsOvertime: true,
						UseLighterBorder: false
					}]
				}]
			};
			var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");
			var schedulesEl = panel[0].querySelector(".shift");
			var shiftLayers = schedulesEl.querySelectorAll(".shift-layer");
			expect(shiftLayers.length).toEqual(4);
			expect(shiftLayers[0].style.width).toEqual('120px');
			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[2].style.width).toEqual('30px');
			expect(shiftLayers[3].style.width).toEqual('30px');

			expect(shiftLayers[0].style.left).toEqual('330px');
			expect(shiftLayers[1].style.left).toEqual('450px');
			expect(shiftLayers[2].style.left).toEqual('510px');
			expect(shiftLayers[3].style.left).toEqual('540px');

			expect(angular.element(shiftLayers[2]).hasClass('overtime')).toBeTruthy();
			expect(angular.element(shiftLayers[2]).hasClass('overtime-light')).toBeTruthy();
			expect(angular.element(shiftLayers[3]).hasClass('overtime')).toBeTruthy();
			expect(angular.element(shiftLayers[3]).hasClass('overtime-dark')).toBeTruthy();

		});

		it('should show all shifts for schedule on DST', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-03-25',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-03-25 05:00",
						Minutes: 120
					}]
				}]
			};
			var panel = setUp(schedule, "2018-03-25", "Europe/Berlin");
			var schedulesEl = panel[0].querySelector(".shift");
			var shiftLayers = schedulesEl.querySelectorAll(".shift-layer");
			expect(shiftLayers.length).toEqual(1);
			expect(shiftLayers[0].style.width).toEqual('120px');
			expect(shiftLayers[0].style.left).toEqual('240px');
		});

		xit('should locate correctly based on the schedule ', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-05-22',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-22 05:00",
						Minutes: 120
					}],
					ProjectionTimeRange: {
						Start: "2018-05-22 05:00",
						End: "2018-05-22 07:00",
					}
				}]
			};
			var panel = setUp(schedule, "2018-05-22", "Europe/Berlin");
			var shiftContainer = panel[0].querySelector(".shift-container");
			expect(shiftContainer.scrollLeft).toEqual('180');
		});

		describe("in locale en-UK", function () {
			beforeEach(function () { moment.locale('en-UK'); });
			afterEach(function () { moment.locale('en'); });

			it('should render time line correctly based on current user locale', function () {
				var panel = setUp({
					Name: 'Agent 1',
					Date: '2018-05-21',
					Shifts: [{
						Projections: [{
							Color: "#80FF80",
							Description: "Phone",
							Start: "2018-03-25 05:00",
							Minutes: 120
						}]
					}]
				}, "2018-05-21", "Europe/Berlin");
				var timeline = panel[0].querySelector(".timeline");
				var intervals = panel[0].querySelectorAll(".timeline .interval");
				var hours = panel[0].querySelectorAll(".timeline .interval .label>span");
				var textHours = [].slice.call(hours).map(function (hourEl) { return hourEl.innerText.trim() });
				expect(!!timeline).toBeTruthy();
				expect(intervals.length).toBe(37);
				expect(textHours).toEqual( ["12:00 AM", "1:00 AM", "2:00 AM", "3:00 AM", "4:00 AM", "5:00 AM", "6:00 AM", "7:00 AM", "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM", "5:00 PM", "6:00 PM", "7:00 PM", "8:00 PM", "9:00 PM", "10:00 PM", "11:00 PM", "12:00 AM", "1:00 AM", "2:00 AM", "3:00 AM", "4:00 AM", "5:00 AM", "6:00 AM", "7:00 AM", "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM"]);
			});
		});

		function setUp(personSchedule, date, timezone) {
			var scope = $rootScope.$new();
			scope.personSchedule = personSchedule;
			scope.date = date;
			scope.timezone = timezone;
			var element = $compile('<shift-editor person-schedule="personSchedule" date="date" timezone="timezone"></shift-editor>')(scope);
			scope.$apply();
			return element;
		}

	});


