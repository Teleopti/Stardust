"use strict";

describe("teamschedule person selection tests", function () {
	var target;
	var scheduleManagement;

	beforeEach(function () {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function (PersonSelection, ScheduleManagement) {
		target = PersonSelection;
		scheduleManagement = ScheduleManagement;
	}));

	var scheduleDate = moment("2016-03-20");
	var personId1 = "221B-Sherlock";
	var personId2 = "221B-SomeoneElse";

	var schedule1 = {
		PersonId: personId1,
		Date: scheduleDate,
		Shifts: [],
		AllowSwap: function () { return false; }
	};

	var newSchedule1 = {
		PersonId: personId1,
		Date: scheduleDate,
		Shifts: [
			{
				Date: scheduleDate,
				AbsenceCount: function () {
					return 1;
				},
				ActivityCount: function () {
					return 1;
				}
			}
		],
		AllowSwap: function () { return false; }
	};

	newSchedule1.Shifts.forEach(function (shift) {
		shift.Parent = newSchedule1;
	});

	var schedule2 = {
		PersonId: personId2,
		Date: scheduleDate,
		Shifts: [],
		AllowSwap: function () { return true; }
	};

	function commonTestsInDifferentLocale() {
		it("Can update correct person info", inject(function () {

			target.updatePersonInfo([schedule1, schedule2]);
			expect(target.isAnyAgentSelected()).toEqual(false);

			target.personInfo[personId1] = {
				Checked: true,
				SelectedAbsences: [],
				SelectedActivities: [],
				PersonAbsenceCount: 0,
				PersonActivityCount: 0,
				AllowSwap: true
			}
			target.updatePersonInfo([schedule1, schedule2]);
			var person1 = target.personInfo[personId1];
			expect(person1.Checked).toEqual(true);
			expect(person1.AllowSwap).toEqual(schedule1.AllowSwap());
			expect(person1.PersonAbsenceCount).toEqual(0);
			expect(person1.PersonActivityCount).toEqual(0);

			target.updatePersonInfo([newSchedule1]);

			person1 = target.personInfo[personId1];
			expect(person1.Checked).toEqual(true);
			expect(person1.AllowSwap).toEqual(newSchedule1.AllowSwap());
			expect(person1.PersonAbsenceCount).toEqual(0);
		}));

		it("Should clear person info", inject(function () {
			target.updatePersonInfo([schedule1]);
			target.clearPersonInfo();
			expect(target.personInfo).toEqual({});
		}));

		it("Should clear all person info extra cached by switching pages", inject(function () {
			target.updatePersonInfo([schedule1, schedule2]);
			target.unselectAllPerson([schedule1]);
			expect(target.personInfo).toEqual({});
		}));

		it("can select/deselect one person", inject(function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2019-02-26',
				"ShiftCategory": {
					"ShortName": "AM",
					"Name": "Early",
					"DisplayColor": "#000000"
				},
				"Projection": [],
				"IsProtected": true,
				"Timezone": { IanaId: "Asia/Shanghai", DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi" },
				"DayOff": null
			};
			scheduleManagement.resetSchedules([schedule], '2019-02-26', "Asia/Shanghai");
			var scheduleVm = scheduleManagement.groupScheduleVm.Schedules[0];

			scheduleVm.IsSelected = true;
			target.updatePersonSelection(scheduleVm);

			var person1 = target.personInfo[schedule.PersonId];
			expect(person1.Checked).toEqual(true);
			expect(target.getSelectedPersonInfoList().length).toEqual(1);

			scheduleVm.IsSelected = false;
			target.updatePersonSelection(scheduleVm);

			expect(target.getSelectedPersonInfoList().length).toEqual(0);
		}));

		it("can select several people", inject(function () {
			schedule1.IsSelected = true;
			schedule2.IsSelected = true;
			target.selectAllPerson([schedule1, schedule2]);

			expect(target.personInfo[personId1].Checked).toEqual(true);
			expect(target.personInfo[personId2].Checked).toEqual(true);
			expect(target.getSelectedPersonInfoList().length).toEqual(2);
		}));

		it("Should get selected person ids", inject(function () {
			schedule1.IsSelected = true;
			schedule2.IsSelected = true;
			target.selectAllPerson([schedule1, schedule2]);
			expect(target.isAnyAgentSelected()).toEqual(true);

			var selectedPersonId = target.getSelectedPersonIdList();
			expect(selectedPersonId.length).toEqual(2);
			expect(selectedPersonId[0]).toEqual(personId1);
			expect(selectedPersonId[1]).toEqual(personId2);
		}));

		it("Should get selected person info", inject(function () {
			newSchedule1.IsSelected = true;
			target.updatePersonSelection(newSchedule1);
			expect(target.isAnyAgentSelected()).toEqual(true);

			var selectedPerson = target.getSelectedPersonInfoList();
			expect(selectedPerson.length).toEqual(1);

			var person = selectedPerson[0];
			expect(person.PersonId).toEqual(personId1);
			expect(person.AllowSwap).toEqual(newSchedule1.AllowSwap());
			expect(person.SelectedAbsences.length).toEqual(0);
		}));

		it('should build person info correctly when there is a day off on tomorrow in view', function () {
			var fakePersonScheduleVm = {
				'Index': 0,
				'ContractTime': '13:00',
				'Date': '2018-02-03',
				'DayOffs': [
					{
						'DayOffName': 'Day off',
						'StartPosition': 77.27272727272728,
						'Length': 22.727272727272727,
						'Date': '2018-02-04'
					}
				],
				'ExtraShifts': [],
				'IsFullDayAbsence': false,
				'MultiplicatorDefinitionSetIds': [
					'29f7ece8-d340-408f-be40-9bb900b8a4cb',
					'9019d62f-0086-44b1-a977-9bb900b8c361'
				],
				'Name': 'Pierre Baldi',
				'PersonId': 'b0e35119-4661-4a1b-8772-9b5e015b2564',
				'ShiftCategory': {
					'Name': 'Day',
					'ShortName': 'DY',
					'DisplayColor': '#FFC080',
					'TextColor': 'black'
				},
				'Shifts': [
					{
						'Date': '2018-02-03',
						'Projections': [
							{
								'Selected': false,
								'ShowDividedLine': false,
								'Color': '#8080FF',
								'Description': 'E-mail',
								'IsOvertime': false,
								'Length': 59.09090909090909,
								'Minutes': 780,
								'ParentPersonAbsences': null,
								'ShiftLayerIds': [
									'811a8c6a-61c2-4cee-bda8-a87900712360'
								],
								'Start': '2018-02-03 08:00',
								'StartPosition': 4.545454545454546,
								'UseLighterBorder': false,
								'Selectable': function () { return true },
								'ToggleSelection': function () { }
							}
						],
						'ProjectionTimeRange': {
							'Start': '2018-02-03 08:00',
							'End': '2018-02-03 21:00'
						},
						'AbsenceCount': function () { return 0; },
						'ActivityCount': function () { return 1; }
					}
				],
				'Timezone': {
					'IanaId': 'Europe/Berlin',
					'DisplayName': '(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna'
				},
				'ViewRange': {
					'startMoment': '2018-02-02T16:00:00.000Z',
					'endMoment': '2018-02-03T22:00:00.000Z'
				},
				'IsSelected': true,
				'AllowSwap': function () { return true },
				'ScheduleStartTime': function () { return '2018-02-03T08:00:00' },
				'ScheduleEndTime': function () { return '2018-02-03T21:00:00' },
				'AbsenceCount': function () { return 0; },
				'ActivityCount': function () { return 1; },
				'MergeExtra': function () { },
				'Merge': function () { }
			};

			target.updatePersonSelection(fakePersonScheduleVm);

			expect(target.personInfo[fakePersonScheduleVm.PersonId].IsDayOff).toEqual(false);
		});
	}

	commonTestsInDifferentLocale();

	describe('in locale ar-AE', function () {
		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	describe('in locale fa-IR', function () {
		beforeAll(function () {
			moment.locale('fa-IR');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});
});
