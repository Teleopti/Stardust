describe('<shift-editor>',
	function () {
		'use strict';
		var $rootScope,
			$compile

		beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
		beforeEach(inject(function (_$rootScope_, _$compile_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
		}));

		it("should render correctly", function () {
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-05-16'
			});
			var element = panel[0];

			expect(element.querySelectorAll('.shift-editor').length).toEqual(1);
			expect(element.querySelector('.shift-editor .name').innerText.trim()).toEqual('Agent 1');
			expect(element.querySelector('.shift-editor .date').innerText).toEqual('2018-05-16');
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

		

		function setUp(personSchedule) {
			var scope = $rootScope.$new();
			var html = '<shift-editor person-schedule="personSchedule"></shift-editor>';
			scope.personSchedule = personSchedule;
			var element = $compile(html)(scope);
			scope.$apply();
			return element;
		}

	});

