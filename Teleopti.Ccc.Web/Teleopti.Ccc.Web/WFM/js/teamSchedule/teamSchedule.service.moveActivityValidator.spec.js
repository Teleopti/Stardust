(function() {
	'use strict';

	describe('test of moveActivityValidator', function() {
		var target, personSelection, scheduleMgmt;
		beforeEach(function () {
			module("wfm.teamSchedule");
		});

		beforeEach(inject(function (PersonSelection, ScheduleManagement, MoveActivityValidator) {
			personSelection = PersonSelection;
			scheduleMgmt = ScheduleManagement;
			target = MoveActivityValidator;
		}));
		var scheduleDate = "2016-05-12";
		var schedule = {
			"PersonId": "221B-Baker-SomeoneElse",
			"Name": "SomeoneElse",
			"Date": scheduleDate,
			"Projection": [
				{
					"ShiftLayerIds": ["layer1"],
					"Color": "#80FF80",
					"Description": "Email",
					"Start": scheduleDate + " 07:00",
					"Minutes": 480
				}
			],
			"IsFullDayAbsence": false,
			"DayOff": null
		};

		xit('should return false when moving to time changes the schedule start date', function() {
			scheduleMgmt.mergeSchedules([schedule], moment(scheduleDate));
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, scheduleDate);
			var newStartMoment = moment("2016-05-13 2:00");

			var result = target.validateMoveToTime(newStartMoment);

			expect(result).toEqual(false);
			expect(target.getInvalidPeople().indexOf('SomeoneElse') > -1).toEqual(true);
		});

		it('should return false when moving to time makes the schedule length longer than 36 hours', function () {
			schedule = {
				"PersonId": "221B-Baker-SomeoneElse",
				"Name": "SomeoneElse",
				"Date": scheduleDate,
				"Projection": [
					{
						"ShiftLayerIds": ["layer1"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Email",
						"Start": scheduleDate + " 01:00",
						"Minutes": 480
					},
					{
						"ShiftLayerIds": ["layer2"],
						"ParentPersonAbsences": null,
						"Color": "#80FF80",
						"Description": "Phone",
						"Start": scheduleDate + " 10:00",
						"Minutes": 480
					}
				],
				"IsFullDayAbsence": false,
				"DayOff": null
			}
			scheduleMgmt.resetSchedules([schedule], moment(scheduleDate));
			var personSchedule = scheduleMgmt.groupScheduleVm.Schedules[0];
			personSchedule.Shifts[0].Projections[1].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[1], personSchedule);
			var newStartMoment = moment("2016-05-13 13:00");

			var result = target.validateMoveToTime(newStartMoment);

			expect(result).toEqual(false);
			expect(target.getInvalidPeople().indexOf('SomeoneElse') > -1).toEqual(true);
		});

	});
})()
