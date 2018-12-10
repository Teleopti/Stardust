(function () {
	"use strict";

	var target;

	describe('#teamschedule.factory.groupSchedule#', function () {
		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				$provide.service('CurrentUserInfo', function () {
					return {
						CurrentUserInfo: function () {
							return {
								DefaultTimeZone: 'Europe/London',
								DateFormatLocale: 'en',
								FirstDayOfWeek: 0,
								DateTimeFormat: {
									ShowMeridian: true,
									ShortTimePattern: 'h:mm A',
									AMDesignator: 'AM',
									PMDesignator: 'PM'
								}
							};
						}
					};
				});
			});
		});

		beforeEach(inject(function (GroupScheduleFactory) {
			target = GroupScheduleFactory;
		}));

		it("can get correct person schedule", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
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

			var scheduleTommorow = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-17',
				"IsFullDayAbsence": true,
				"Projection": [
					{
						"ActivityId": "47d9292f-ead6-40b2-ac4f-9b5e015ab330",
						"ShiftLayerIds": null,
						"ParentPersonAbsences": ["bc63cf37-e243-4adc-8456-a97b0085c70a"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 08:00",
						"EndInUtc": "2018-10-17 16:00",
						"IsOvertime": false
					}
				],
				"IsProtected": true,
				"Timezone": {
					IanaId: "Asia/Shanghai",
					DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
				},
				"DayOff": null
			};

			var personSchedule = target.Create([schedule, scheduleTommorow], '2018-10-16', 'etc/UTC').Schedules[0];

			expect(personSchedule.Date).toEqual("2018-10-16");
			expect(personSchedule.PersonId).toEqual("221B-Baker-Street");
			expect(personSchedule.Name).toEqual("Sherlock Holmes");
			expect(!!personSchedule.IsFullDayAbsence).toEqual(false);
			expect(personSchedule.AllowSwap()).toEqual(true);
			expect(personSchedule.Timezone.IanaId).toEqual('Asia/Shanghai');
			expect(personSchedule.IsProtected).toEqual(true);
			expect(personSchedule.ShiftCategory.ShortName).toEqual('AM');
			expect(personSchedule.ShiftCategory.Name).toEqual('Early');
			expect(personSchedule.ShiftCategory.DisplayColor).toEqual('#000000');
			expect(personSchedule.ShiftCategory.TextColor).toEqual('white');
			expect(personSchedule.DayOffs.length).toEqual(0);

			var personScheduleTommorow = target.Create([schedule, scheduleTommorow], '2018-10-17', 'etc/UTC').Schedules[0];
			expect(personScheduleTommorow.AllowSwap()).toEqual(false);
		});

		it("can get person schedule with correct projection", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"ShiftCategory": {
					"ShortName": "AM",
					"Name": "Early",
					"DisplayColor": "#000000"
				},
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-16 08:00",
						"EndInUtc": "2018-10-16 10:00",
						"IsOvertime": false
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-16 10:00",
						"EndInUtc": "2018-10-16 12:00",
						"IsOvertime": true
					},
					{
						"ShiftLayerIds": null,
						"Color": "#FFFFFF",
						"Description": "Holiday",
						"StartInUtc": "2018-10-16 12:00",
						"EndInUtc": "2018-10-16 13:00",
						"ParentPersonAbsences": ['41ffe214-3384-4a80-a14c-a83800e23276']
					}
				],
				"IsProtected": true,
				"Timezone": {
					IanaId: "Asia/Shanghai",
					DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
				},
				"DayOff": null
			};

			var personSchedule = target.Create([schedule], '2018-10-16', 'Asia/Shanghai').Schedules[0];

			expect(personSchedule.Shifts[0].Projections[0].ShiftLayerIds).toEqual(["31ffe214-3384-4a80-a14c-a83800e23276"]);
			expect(personSchedule.Shifts[0].Projections[0].Color).toEqual("#795548");
			expect(personSchedule.Shifts[0].Projections[0].Description).toEqual("Phone");
			expect(personSchedule.Shifts[0].Projections[0].TimeSpan).toEqual("4:00 PM - 6:00 PM");
			expect(personSchedule.Shifts[0].Projections[0].UseLighterBorder).toEqual(true);
			expect(personSchedule.Shifts[0].Projections[0].Selectable()).toEqual(true);

			expect(personSchedule.Shifts[0].Projections[1].ShiftLayerIds).toEqual(["41ffe214-3384-4a80-a14c-a83800e23276"]);
			expect(personSchedule.Shifts[0].Projections[1].Color).toEqual("#FFFFFF");
			expect(personSchedule.Shifts[0].Projections[1].Description).toEqual("Email");
			expect(personSchedule.Shifts[0].Projections[1].TimeSpan).toEqual("6:00 PM - 8:00 PM");
			expect(personSchedule.Shifts[0].Projections[1].UseLighterBorder).toEqual(false);
			expect(personSchedule.Shifts[0].Projections[1].Selectable()).toEqual(true);

			expect(personSchedule.Shifts[0].Projections[2].ShiftLayerIds).toEqual(null);
			expect(personSchedule.Shifts[0].Projections[2].Color).toEqual("#FFFFFF");
			expect(personSchedule.Shifts[0].Projections[2].Description).toEqual("Holiday");
			expect(personSchedule.Shifts[0].Projections[2].TimeSpan).toEqual("8:00 PM - 9:00 PM");
			expect(personSchedule.Shifts[0].Projections[2].UseLighterBorder).toEqual(false);
			expect(personSchedule.Shifts[0].Projections[2].Selectable()).toEqual(true);

		});

		it('should get projection with correct length and start position ', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-16 08:00",
						"EndInUtc": "2018-10-16 10:00"
					}
				],
				"IsProtected": true,
				"Timezone": {
					IanaId: "Asia/Shanghai",
					DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
				},
				"DayOff": null
			};

			var vm = target.Create([schedule], '2018-10-16', 'etc/UTC');
			expect(vm.Schedules[0].Shifts[0].Projections[0].StartPosition).toBe(60 * vm.TimeLine.LengthPercentPerMinute);
			expect(vm.Schedules[0].Shifts[0].Projections[0].Length).toBe(120 * vm.TimeLine.LengthPercentPerMinute);
		});

		it('should set start position to 0 when having overnight shift from yesterday ', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-16 22:00",
						"EndInUtc": "2018-10-17 10:00"
					}
				],
				"DayOff": null
			};

			var personSchedule = target.Create([schedule], '2018-10-17', 'etc/UTC').Schedules[0];
			expect(personSchedule.Shifts[0].Projections[0].StartPosition).toBe(0);
		});

		it('should truncate projection when having projection ends later than next 6:00 AM', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 22:00",
						"EndInUtc": "2018-10-18 08:00"
					}
				],
				"DayOff": null
			};

			var vm = target.Create([schedule], '2018-10-17', 'etc/UTC');
			expect(vm.Schedules[0].Shifts[0].Projections[0].Length).toBe(480 * vm.TimeLine.LengthPercentPerMinute);
		});

		it('should get correct projection length and timespan when having 2-hour projection starts at 2018-10-28 1:00 under Europe/London timezone', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-28',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-28 00:00",
						"EndInUtc": "2018-10-28 02:00"
					}
				],
				"DayOff": null
			};

			var vm = target.Create([schedule], '2018-10-28', 'Europe/London');
			expect(vm.Schedules[0].Shifts[0].Projections[0].Length).toBe(120 * vm.TimeLine.LengthPercentPerMinute);
			expect(vm.Schedules[0].Shifts[0].Projections[0].TimeSpan).toBe('1:00 AM - 2:00 AM');
		})

		it('should get projection with correct length, start position and timespan on end of DST', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-28',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-27 23:00",
						"EndInUtc": "2018-10-28 00:30"
					},
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-28 00:30",
						"EndInUtc": "2018-10-28 01:00"
					},
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-28 01:00",
						"EndInUtc": "2018-10-28 03:00"
					}
				],
				"DayOff": null
			};

			var vm = target.Create([schedule], '2018-10-28', 'Europe/London');
			var personSchedule = vm.Schedules[0];
			expect(personSchedule.Shifts[0].Projections[0].StartPosition).toBe(0);
			expect(personSchedule.Shifts[0].Projections[0].Length).toBe(vm.TimeLine.LengthPercentPerMinute * 90);
			expect(personSchedule.Shifts[0].Projections[0].TimeSpan).toBe('12:00 AM - 1:30 AM');

			expect(personSchedule.Shifts[0].Projections[1].StartPosition).toBe(vm.TimeLine.LengthPercentPerMinute * 90);
			expect(personSchedule.Shifts[0].Projections[1].Length).toBe(vm.TimeLine.LengthPercentPerMinute * 30);
			expect(personSchedule.Shifts[0].Projections[1].TimeSpan).toBe('1:30 AM - 1:00 AM');

			expect(personSchedule.Shifts[0].Projections[2].StartPosition).toBe(vm.TimeLine.LengthPercentPerMinute * 120);
			expect(personSchedule.Shifts[0].Projections[2].Length).toBe(vm.TimeLine.LengthPercentPerMinute * 120);
			expect(personSchedule.Shifts[0].Projections[2].TimeSpan).toBe('1:00 AM - 3:00 AM');
		});

		it('should get projection with correct length, start position and timespan on start of DST', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-03-25',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-03-25 01:00",
						"EndInUtc": "2018-03-25 03:00"
					}
				],
				"DayOff": null
			};

			var vm = target.Create([schedule], '2018-03-25', 'Europe/Berlin');
			var personSchedule = vm.Schedules[0];
			expect(personSchedule.Shifts[0].Projections[0].StartPosition).toBe(vm.TimeLine.LengthPercentPerMinute * 60);
			expect(personSchedule.Shifts[0].Projections[0].Length).toBe(vm.TimeLine.LengthPercentPerMinute * 120);
		});

		it("should get  person schedule with correct day off", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-16",
				"Projection": [],
				"DayOff":
					{
						"DayOffName": "DayOff",
						"StartInUtc": "2018-10-16 00:00",
						"EndInUtc": "2018-10-17 00:00",
						"Minutes": 1440
					}
			};
			var vm = target.Create([schedule], "2018-10-16", "etc/UTC");
			var personSchedule = vm.Schedules[0];

			expect(personSchedule.DayOffs.length).toEqual(1);
			expect(personSchedule.DayOffs[0].DayOffName).toEqual("DayOff");
			expect(personSchedule.DayOffs[0].Length).toEqual(vm.TimeLine.LengthPercentPerMinute * 480);
			expect(personSchedule.DayOffs[0].StartPosition).toEqual(0);
		});

		it("should get day off with correct length and start position when there're agents in different time zone", function () {
			var scheduleForPerson1 = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-16",
				"Projection": [],
				"DayOff":
					{
						"DayOffName": "DayOff",
						"StartInUtc": "2018-10-16 16:00",
						"EndInUtc": "2018-10-17 16:00"
					}
			};

			var scheduleForPerson2 = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-16",
				"Projection": [{
					"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
					"Color": "#795548",
					"Description": "Phone",
					"StartInUtc": "2018-10-16 14:00",
					"EndInUtc": "2018-10-16 18:00"
				}]
			};

			var vm = target.Create([scheduleForPerson1, scheduleForPerson2], "2018-10-16", "Asia/Hong_Kong")
			var personSchedule = vm.Schedules[0];

			expect(personSchedule.DayOffs.length).toEqual(1);
			expect(personSchedule.DayOffs[0].DayOffName).toEqual("DayOff");
			expect(personSchedule.DayOffs[0].Length).toEqual(vm.TimeLine.LengthPercentPerMinute * 180);
			expect(personSchedule.DayOffs[0].StartPosition).toEqual(vm.TimeLine.LengthPercentPerMinute * 180);
		});

		it("can get person schedule with full day absence", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"IsFullDayAbsence": true,
				"Projection": [
					{
						"ActivityId": "47d9292f-ead6-40b2-ac4f-9b5e015ab330",
						"ShiftLayerIds": null,
						"ParentPersonAbsences": ["bc63cf37-e243-4adc-8456-a97b0085c70a"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-16 08:00",
						"EndInUtc": "2018-10-16 10:00",
						"IsOvertime": false
					}
				],
				"IsProtected": true,
				"Timezone": {
					IanaId: "Asia/Shanghai",
					DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
				},
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], "2018-10-16", "etc/UTC").Schedules[0];

			expect(personSchedule.IsFullDayAbsence).toBe(true);
			expect(personSchedule.Shifts[0].Projections[0].Color).toEqual('#795548');
			expect(personSchedule.Shifts[0].Projections[0].UseLighterBorder).toEqual(true);
			expect(personSchedule.Shifts[0].Projections[0].ParentPersonAbsences).toEqual(["bc63cf37-e243-4adc-8456-a97b0085c70a"]);
			expect(personSchedule.Shifts[0].Projections[0].TimeSpan).toEqual('8:00 AM - 10:00 AM');
			expect(personSchedule.Shifts[0].Projections[0].Description).toEqual('Phone');
			expect(personSchedule.Shifts[0].Projections[0].Selectable()).toEqual(true);
		});

		it('can get person schedule with correct formatted contract time', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-17",
				"ContractTimeMinutes": 480,
				"Projection": [
					{
						"ShiftLayerIds": ["222"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 07:00",
						"EndInUtc": "2018-10-17 11:00"
					},
					{
						"ShiftLayerIds": ["333"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 11:00",
						"EndInUtc": "2018-10-18 07:30"
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], "2018-10-17", 'etc/UTC').Schedules[0];
			expect(personSchedule.ContractTime).toEqual("8:00");
		});

		it('can get person schedule with correct formatted contact time when it is greater than 24 hours', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-17",
				"ContractTimeMinutes": 1470,
				"Projection": [
					{
						"ShiftLayerIds": ["222"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 07:00",
						"EndInUtc": "2018-10-18 07:30",
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], "2018-10-17", "etc/UTC").Schedules[0];
			expect(personSchedule.ContractTime).toEqual("24:30");
		});

		it('should get correct regular activities count and exclude overtime activities, intraday absences', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["222"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 07:00",
						"EndInUtc": "2018-10-17 11:00"
					},
					{
						"ShiftLayerIds": null,
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 07:00",
						"EndInUtc": "2018-10-17 11:00",
						"ParentPersonAbsences": ['41ffe214-3384-4a80-a14c-a83800e23276']
					},
					{
						"ShiftLayerIds": ["333"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 11:00",
						"EndInUtc": "2018-10-17 15:00",
						"IsOvertime": true
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], '2018-10-17', 'ETC/UTC').Schedules[0];
			expect(personSchedule.ActivityCount()).toEqual(1);

		});

		it('should get correct regular activities count after time zone conversion', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["222"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 22:00",
						"EndInUtc": "2018-10-17 23:00"
					},
					{
						"ShiftLayerIds": ["333"],
						"Color": "#80FFFF",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 23:00",
						"EndInUtc": "2018-10-17 23:30"
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], '2018-10-17', 'Asia/Hong_Kong').Schedules[0];
			expect(personSchedule.ActivityCount()).toEqual(0);
		});

		it('should get person schedule with correct absence count', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["222"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 07:00",
						"EndInUtc": "2018-10-17 11:00"
					},
					{
						"ShiftLayerIds": null,
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 07:00",
						"EndInUtc": "2018-10-17 11:00",
						"ParentPersonAbsences": ['41ffe214-3384-4a80-a14c-a83800e23276']
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], '2018-10-17', 'ETC/UTC').Schedules[0];
			expect(personSchedule.AbsenceCount()).toBe(1);
		});

		it('should get person schedule with correct absence count after time zone conversion', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["222"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 19:00",
						"EndInUtc": "2018-10-17 23:00"
					},
					{
						"ShiftLayerIds": null,
						"Color": "#80FF80",
						"Description": "Holiday",
						"StartInUtc": "2018-10-17 22:00",
						"EndInUtc": "2018-10-17 23:00",
						"ParentPersonAbsences": ['41ffe214-3384-4a80-a14c-a83800e23276']
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], '2018-10-17', 'Asia/Hong_Kong').Schedules[0];
			expect(personSchedule.AbsenceCount()).toBe(0);
		});

		it("should merge yesterdays overnight projection to selected days schedule view model", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"ShiftCategory": {
					"ShortName": "AM",
					"Name": "Early",
					"DisplayColor": "#000000"
				},
				"Projection": [{
					"Color": "Red",
					"Description": "Phone",
					"StartInUtc": "2018-10-16 07:00",
					"EndInUtc": "2018-10-16 15:00"
				}],
				"IsProtected": true,
				"Timezone": { IanaId: "Etc/Utc", DisplayName: "UTC" },
				"DayOff": null
			};

			var yesterdaySchedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-15",
				"Projection": [
					{
						"Color": "Red",
						"Description": "Phone",
						"StartInUtc": "2018-10-15 17:00",
						"EndInUtc": "2018-10-15 20:00"
					},
					{
						"Color": "Red",
						"Description": "Email",
						"StartInUtc": "2018-10-15 20:00",
						"EndInUtc": "2018-10-16 04:00"
					}
				],
				DayOff: null
			};

			var personSchedule = target.Create([schedule, yesterdaySchedule], '2018-10-16', 'etc/UTC').Schedules[0];

			expect(personSchedule.Shifts.length).toEqual(2);
			expect(personSchedule.Shifts[0].Date).toEqual('2018-10-16');
			expect(personSchedule.Shifts[0].Projections.length).toEqual(1);
			expect(personSchedule.Shifts[0].Projections[0].TimeSpan).toEqual('7:00 AM - 3:00 PM');

			expect(personSchedule.Shifts[1].Date).toEqual('2018-10-15');
			expect(personSchedule.Shifts[1].Projections.length).toEqual(1);
			expect(personSchedule.Shifts[1].Projections[0].TimeSpan).toEqual('10/15/2018 8:00 PM - 10/16/2018 4:00 AM');
		});

		it("should merge yesterdays projection which falls in viewing time range to selected days schedule view model after timezone conversion ", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"ShiftCategory": {
					"ShortName": "AM",
					"Name": "Early",
					"DisplayColor": "#000000"
				},
				"Projection": [{
					"Color": "Red",
					"Description": "Phone",
					"StartInUtc": "2018-10-16 23:00",
					"EndInUtc": "2018-10-16 23:30"
				}],
				"IsProtected": true,
				"Timezone": { IanaId: "Etc/Utc", DisplayName: "UTC" },
				"DayOff": null
			};

			var yesterdaySchedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-15",
				"Projection": [
					{
						"Color": "Red",
						"Description": "Phone",
						"StartInUtc": "2018-10-15 17:00",
						"EndInUtc": "2018-10-15 20:00"
					},
					{
						"Color": "Red",
						"Description": "Email",
						"StartInUtc": "2018-10-15 20:00",
						"EndInUtc": "2018-10-16 04:00"
					}
				],
				DayOff: null
			};

			var personSchedule = target.Create([schedule, yesterdaySchedule], '2018-10-16', 'Asia/Hong_Kong').Schedules[0];
			expect(personSchedule.Shifts.length).toEqual(2);
			expect(personSchedule.Shifts[0].Date).toEqual('2018-10-16');
			expect(personSchedule.Shifts[0].Projections.length).toEqual(0);

			expect(personSchedule.Shifts[1].Date).toEqual('2018-10-15');
			expect(personSchedule.Shifts[1].Projections.length).toEqual(2);
			expect(personSchedule.Shifts[1].Projections[0].TimeSpan).toEqual('1:00 AM - 4:00 AM');
			expect(personSchedule.Shifts[1].Projections[1].TimeSpan).toEqual('4:00 AM - 12:00 PM');
		});

		it("should not merge yesterdays shift to selected days schedule view model if no projection in viewing time range ", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"ShiftCategory": {
					"ShortName": "AM",
					"Name": "Early",
					"DisplayColor": "#000000"
				},
				"Projection": [{
					"Color": "Red",
					"Description": "Phone",
					"StartInUtc": "2018-10-16 07:00",
					"EndInUtc": "2018-10-16 15:00"
				}],
				"IsProtected": true,
				"Timezone": { IanaId: "Etc/Utc", DisplayName: "UTC" },
				"DayOff": null
			};

			var yesterdaySchedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-15",
				"Projection": [
					{
						"Color": "Red",
						"Description": "Phone",
						"StartInUtc": "2018-10-15 17:00",
						"EndInUtc": "2018-10-15 20:00"
					}
				],
				DayOff: null
			};

			var personSchedule = target.Create([schedule, yesterdaySchedule], '2018-10-16', 'ETC/UTC').Schedules[0];

			expect(personSchedule.Shifts.length).toEqual(1);
			expect(personSchedule.Shifts[0].Date).toEqual('2018-10-16');
			expect(personSchedule.Shifts[0].Projections.length).toEqual(1);
			expect(personSchedule.Shifts[0].Projections[0].TimeSpan).toEqual('7:00 AM - 3:00 PM');
		});

		it("should merge dayoff", function () {
			var yesterdaySchedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-16",
				"Projection": [],
				"DayOff":
					{
						"DayOffName": "Day off",
						"StartInUtc": "2018-10-16 00:00",
						"EndInUtc": "2018-10-17 00:00"
					}
			};

			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-17",
				"Projection": [],
				"DayOff":
					{
						"DayOffName": "Shoft DayOff",
						"StartInUtc": "2018-10-17 00:00",
						"EndInUtc": "2018-10-18 00:00",
					}
			};

			var personSchedule = target.Create([schedule, yesterdaySchedule], '2018-10-17', 'etc/UTC').Schedules[0];

			expect(personSchedule.Date).toEqual('2018-10-17');
			expect(personSchedule.Shifts.length).toEqual(0);
			expect(personSchedule.DayOffs.length).toEqual(1);
		});

		it("should  merge yesterdays shift to selected days schedule view models extra shifts if no projection in viewing time range ", function () {
			var yesterdaySchedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-15",
				"Projection": [
					{
						"Color": "Red",
						"Description": "Phone",
						"StartInUtc": "2018-10-15 17:00",
						"EndInUtc": "2018-10-15 20:00"
					}
				],
				DayOff: null
			};

			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"Projection": [{
					"Color": "Red",
					"Description": "Phone",
					"StartInUtc": "2018-10-16 07:00",
					"EndInUtc": "2018-10-16 15:00"
				}],
				"IsProtected": true,
				"Timezone": { IanaId: "Etc/Utc", DisplayName: "UTC" },
				"DayOff": null
			};

			var personSchedule = target.Create([schedule, yesterdaySchedule], '2018-10-16', 'ETC/UTC').Schedules[0];

			expect(personSchedule.ExtraShifts.length).toEqual(1);
			expect(personSchedule.ExtraShifts[0].Date).toEqual("2018-10-15");
			expect(personSchedule.ExtraShifts[0].ProjectionTimeRange.Start).toEqual("2018-10-15 17:00");
			expect(personSchedule.ExtraShifts[0].ProjectionTimeRange.End).toEqual("2018-10-15 20:00");
		});

		it('should show divided line if the activity same with another same type personal activity', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-16',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-16 08:00",
						"EndInUtc": "2018-10-16 10:00",
						"ActivityId": "47d9292f-ead6-40b2-ac4f-9b5e015ab330"
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-16 10:00",
						"EndInUtc": "2018-10-16 11:00",
						"IsPersonalActivity": true,
						"ActivityId": "47d9292f-ead6-40b2-ac4f-9b5e015ab330"
					},
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23xxx"],
						"Color": "#795548",
						"Description": "EMail",
						"StartInUtc": "2018-10-16 11:00",
						"EndInUtc": "2018-10-16 12:00",
						"ActivityId": "47d9292f-ead6-40b2-ac4f-9b5e015abxxx"
					}
				]
			};

			var personSchedule = target.Create([schedule], '2018-10-16', 'etc/UTC').Schedules[0];
			expect(personSchedule.Shifts[0].Projections[0].ShowDividedLine).toBe(false);
			expect(personSchedule.Shifts[0].Projections[1].ShowDividedLine).toBe(true);
			expect(personSchedule.Shifts[0].Projections[2].ShowDividedLine).toBe(false);
		});

		it('should get correct formatted underlying schedule timespan', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-16",
				"ContractTimeMinutes": 480,
				"Projection": [
					{
						"ShiftLayerIds": ["222"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-16 07:00",
						"EndInUtc": "2018-10-16 11:00"
					},
					{
						"ShiftLayerIds": ["333"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-16 11:00",
						"End": "2018-10-16 16:00"
					}],
				"DayOff": null,
				"UnderlyingScheduleSummary": {
					"PersonalActivities": [{
						"Description": "personal activity",
						"StartInUtc": '2018-10-16 10:00',
						"EndInUtc": '2018-10-16 11:00'
					}],
					"PersonPartTimeAbsences": [{
						"Description": "holiday",
						"StartInUtc": '2018-10-16 11:30',
						"EndInUtc": '2018-10-16 12:00'
					}],
					"PersonMeetings": [{
						"Description": "administration",
						"StartInUtc": '2018-10-16 14:00',
						"EndInUtc": '2018-10-16 15:00'
					}]
				}
			};
			var personScheduleVm = target.Create([schedule], '2018-10-16', 'ETC/UTC').Schedules[0];
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonalActivities[0].TimeSpan).toEqual("10:00 AM - 11:00 AM");
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].TimeSpan).toEqual("11:30 AM - 12:00 PM");
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonMeetings[0].TimeSpan).toEqual("2:00 PM - 3:00 PM");
		});

		it('should get correct formatted underlying schedule timespan when timespan date is different from quering date after switching timezone', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-16",
				"ContractTimeMinutes": 480,
				"DayOff": null,
				"Projection": [],
				"UnderlyingScheduleSummary": {
					"PersonalActivities": [{
						"Description": "personal activity",
						"StartInUtc": '2018-10-16 19:00',
						"EndInUtc": '2018-10-16 20:00'
					}],
					"PersonPartTimeAbsences": [{
						"Description": "holiday",
						"StartInUtc": '2018-10-16 19:30',
						"EndInUtc": '2018-10-16 20:00'
					}],
					"PersonMeetings": [{
						"Description": "administration",
						"StartInUtc": '2018-10-16 21:00',
						"EndInUtc": '2018-10-16 22:00'
					}]
				}
			};
			var personScheduleVm = target.Create([schedule], '2018-10-16', 'Asia/Hong_Kong').Schedules[0];
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonalActivities[0].TimeSpan).toEqual("10/17/2018 3:00 AM - 10/17/2018 4:00 AM");
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].TimeSpan).toEqual("10/17/2018 3:30 AM - 10/17/2018 4:00 AM");
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonMeetings[0].TimeSpan).toEqual("10/17/2018 5:00 AM - 10/17/2018 6:00 AM");
		});

		it('should get correct formatted underlying schedule timespan when start and end on different date', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-16",
				"ContractTimeMinutes": 480,
				"DayOff": null,
				"Projection": [],
				"UnderlyingScheduleSummary": {
					"PersonalActivities": [{
						"Description": "personal activity",
						"StartInUtc": '2018-10-16 19:00',
						"EndInUtc": '2018-10-17 02:00'
					}]
				}
			};
			var personScheduleVm = target.Create([schedule], '2018-10-16', 'etc/UTC').Schedules[0];
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonalActivities[0].TimeSpan).toEqual("10/16/2018 7:00 PM - 10/17/2018 2:00 AM");
		});

		it("should get correct schedule start time moment and end time moment", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": '2018-10-17',
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": '2018-10-17 07:00',
						"EndInUtc": '2018-10-17 08:00'
					}
				],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], '2018-10-17', 'ETC/UTC').Schedules[0];

			expect(personSchedule.ScheduleStartTimeMoment().format('YYYY-MM-DD HH:mm')).toEqual('2018-10-17 07:00');
			expect(personSchedule.ScheduleEndTimeMoment().format('YYYY-MM-DD HH:mm')).toEqual("2018-10-17 08:00");
		});

		it("should get correct schedule start time moment and end time moment after timezone convertion", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-17",
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 07:00",
						"EndInUtc": "2018-10-17 15:00"
					},
					{
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-17 23:00",
						"EndInUtc": "2018-10-17 23:30",
						"IsOvertime": true
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], '2018-10-17', 'Asia/Hong_Kong').Schedules[0];
			expect(personSchedule.ScheduleStartTimeMoment().format('YYYY-MM-DD HH:mm')).toEqual('2018-10-17 15:00');
			expect(personSchedule.ScheduleEndTimeMoment().format('YYYY-MM-DD HH:mm')).toEqual("2018-10-17 23:00");
		});

		it("should set hidden schedule start to true if yesterday's shift is overnight", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-18",
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-18 22:00",
						"EndInUtc": "2018-10-19 06:00"
					},
					{
						"Color": "#FFFFFF",
						"Description": "Phone",
						"StartInUtc": "2018-10-19 06:00",
						"EndInUtc": "2018-10-19 08:00"
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], '2018-10-19', 'ETC/UTC').Schedules[0];
			expect(personSchedule.HasHiddenScheduleAtStart()).toBe(true);
		});

		it("should set hidden schedule end to true if has overnight shift", function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-18",
				"Projection": [
					{
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-18 22:00",
						"EndInUtc": "2018-10-19 06:00"
					},
					{
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-19 06:00",
						"EndInUtc": "2018-10-19 07:00"
					}],
				"DayOff": null
			};
			var personSchedule = target.Create([schedule], '2018-10-18', 'ETC/UTC').Schedules[0];
			expect(personSchedule.HasHiddenScheduleAtEnd()).toBe(true);
		});


	});

	describe("# time line #", function () {
		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				$provide.service('CurrentUserInfo', function () {
					return {
						CurrentUserInfo: function () {
							return {
								DefaultTimeZone: 'Europe/London',
								DateFormatLocale: 'en',
								FirstDayOfWeek: 0,
								DateTimeFormat: {
									ShowMeridian: true,
									ShortTimePattern: 'h:mm A',
									AMDesignator: 'AM',
									PMDesignator: 'PM'
								}
							};
						}
					};
				});
			});
		});

		beforeEach(inject(function (GroupScheduleFactory) {
			target = GroupScheduleFactory;
		}));

		it('should get correct timeline', function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 08:00",
						"EndInUtc": "2018-10-17 09:00",
						"IsOvertime": false
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-17 09:00",
						"EndInUtc": "2018-10-17 10:00"
					}
				],
				"DayOff": null
			};

			var timelineVm = target.Create([scheduleForPerson1], '2018-10-17', 'ETC/UTC').TimeLine;

			expect(timelineVm.Offset.format("YYYY-MM-DD HH:mm:ss")).toEqual("2018-10-17 00:00:00");
			expect(timelineVm.StartMinute).toEqual(420);
			expect(timelineVm.EndMinute).toEqual(660);
			expect(timelineVm.LengthPercentPerMinute).toEqual(new Number(100 / (660 - 420)).toFixed(3));
		});

		it("should display 1 extra hour line when schedule starts or ends at hour point", function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 08:00",
						"EndInUtc": "2018-10-17 09:00",
						"IsOvertime": false
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-17 09:00",
						"EndInUtc": "2018-10-17 10:00"
					}
				],
				"DayOff": null
			};

			var scheduleForPerson2 = {
				"PersonId": "person2",
				"Name": "person2",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 12:00",
						"EndInUtc": "2018-10-17 16:00"
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-17 16:00",
						"EndInUtc": "2018-10-17 20:00"
					}
				],
				"DayOff": null
			};

			var timelineVm = target.Create([scheduleForPerson1, scheduleForPerson2], '2018-10-17', 'ETC/UTC').TimeLine;

			expect(timelineVm.HourPoints.length).toEqual(15);

			var firstHourPoint = timelineVm.HourPoints[0];
			expect(firstHourPoint.TimeLabel).toEqual("7:00 AM");
			expect(firstHourPoint.Position()).toEqual(0);

			var lastHourPoint = timelineVm.HourPoints[timelineVm.HourPoints.length - 1];
			expect(lastHourPoint.TimeLabel).toEqual("9:00 PM");
			expect(lastHourPoint.Position()).toEqual(840 * timelineVm.LengthPercentPerMinute);
		});

		it('should show all time labels when time range length is less or equal to 16 hours', function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-18',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-18 08:00",
						"EndInUtc": "2018-10-18 15:00",
						"IsOvertime": false
					}
				],
				"DayOff": null
			};

			var timelineVm = target.Create([scheduleForPerson1], '2018-10-18', 'etc/UTC').TimeLine;
			expect(timelineVm.HourPoints.length).toEqual(10);

			expect(timelineVm.HourPoints[0].TimeLabel).toEqual('7:00 AM');
			expect(timelineVm.HourPoints[0].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[1].TimeLabel).toEqual('8:00 AM');
			expect(timelineVm.HourPoints[1].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[2].TimeLabel).toEqual('9:00 AM');
			expect(timelineVm.HourPoints[2].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[3].TimeLabel).toEqual('10:00 AM');
			expect(timelineVm.HourPoints[3].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[4].TimeLabel).toEqual('11:00 AM');
			expect(timelineVm.HourPoints[4].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[5].TimeLabel).toEqual('12:00 PM');
			expect(timelineVm.HourPoints[5].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[6].TimeLabel).toEqual('1:00 PM');
			expect(timelineVm.HourPoints[6].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[7].TimeLabel).toEqual('2:00 PM');
			expect(timelineVm.HourPoints[7].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[8].TimeLabel).toEqual('3:00 PM');
			expect(timelineVm.HourPoints[8].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[9].TimeLabel).toEqual('4:00 PM');
			expect(timelineVm.HourPoints[9].IsLabelVisible).toEqual(true);
		});

		it('should show interval time labels when schedules length is larger than 16 hours', function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-18',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-18 04:00",
						"EndInUtc": "2018-10-18 21:00",
						"IsOvertime": false
					}
				],
				"DayOff": null
			};

			var timelineVm = target.Create([scheduleForPerson1], '2018-10-18', 'etc/UTC').TimeLine;
			expect(timelineVm.HourPoints.length).toEqual(20);

			expect(timelineVm.HourPoints[0].TimeLabel).toEqual('3:00 AM');
			expect(timelineVm.HourPoints[0].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[1].TimeLabel).toEqual('4:00 AM');
			expect(timelineVm.HourPoints[1].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[2].TimeLabel).toEqual('5:00 AM');
			expect(timelineVm.HourPoints[2].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[3].TimeLabel).toEqual('6:00 AM');
			expect(timelineVm.HourPoints[3].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[4].TimeLabel).toEqual('7:00 AM');
			expect(timelineVm.HourPoints[4].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[5].TimeLabel).toEqual('8:00 AM');
			expect(timelineVm.HourPoints[5].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[6].TimeLabel).toEqual('9:00 AM');
			expect(timelineVm.HourPoints[6].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[7].TimeLabel).toEqual('10:00 AM');
			expect(timelineVm.HourPoints[7].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[8].TimeLabel).toEqual('11:00 AM');
			expect(timelineVm.HourPoints[8].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[9].TimeLabel).toEqual('12:00 PM');
			expect(timelineVm.HourPoints[9].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[10].TimeLabel).toEqual('1:00 PM');
			expect(timelineVm.HourPoints[10].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[11].TimeLabel).toEqual('2:00 PM');
			expect(timelineVm.HourPoints[11].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[12].TimeLabel).toEqual('3:00 PM');
			expect(timelineVm.HourPoints[12].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[13].TimeLabel).toEqual('4:00 PM');
			expect(timelineVm.HourPoints[13].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[14].TimeLabel).toEqual('5:00 PM');
			expect(timelineVm.HourPoints[14].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[15].TimeLabel).toEqual('6:00 PM');
			expect(timelineVm.HourPoints[15].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[16].TimeLabel).toEqual('7:00 PM');
			expect(timelineVm.HourPoints[16].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[17].TimeLabel).toEqual('8:00 PM');
			expect(timelineVm.HourPoints[17].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[18].TimeLabel).toEqual('9:00 PM');
			expect(timelineVm.HourPoints[18].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[19].TimeLabel).toEqual('10:00 PM');
			expect(timelineVm.HourPoints[19].IsLabelVisible).toEqual(false);
		});

		it("should get correct time line 8:00 to 16:00 when shift is empty or day off", function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-18',
				"Projection": [],
				"DayOff": null
			};

			var scheduleForPerson2 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-18',
				"Projection": [],
				"DayOff": {
					"DayOffName": "Day off",
					"StartInUtc": "2018-10-18 00:00",
					"EndInUtc": "2018-10-19 00:00"
				}
			};

			var timelineVm = target.Create([scheduleForPerson1, scheduleForPerson2], '2018-10-18', 'ETC/UTC').TimeLine;

			expect(timelineVm.StartMinute).toEqual(480);
			expect(timelineVm.EndMinute).toEqual(960);

			expect(timelineVm.HourPoints.length).toBe(9);

			expect(timelineVm.HourPoints[0].TimeLabel).toEqual('8:00 AM');
			expect(timelineVm.HourPoints[0].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[1].TimeLabel).toEqual('9:00 AM');
			expect(timelineVm.HourPoints[1].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[2].TimeLabel).toEqual('10:00 AM');
			expect(timelineVm.HourPoints[2].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[3].TimeLabel).toEqual('11:00 AM');
			expect(timelineVm.HourPoints[3].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[4].TimeLabel).toEqual('12:00 PM');
			expect(timelineVm.HourPoints[4].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[5].TimeLabel).toEqual('1:00 PM');
			expect(timelineVm.HourPoints[5].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[6].TimeLabel).toEqual('2:00 PM');
			expect(timelineVm.HourPoints[6].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[7].TimeLabel).toEqual('3:00 PM');
			expect(timelineVm.HourPoints[7].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[8].TimeLabel).toEqual('4:00 PM');
			expect(timelineVm.HourPoints[8].IsLabelVisible).toEqual(true);

		});

		it("should get time line on next day when having shift starts early than 6:00 on the next day ", function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-18',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-18 04:00",
						"EndInUtc": "2018-10-18 07:00",
						"IsOvertime": false
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-18 07:00",
						"EndInUtc": "2018-10-18 11:00"
					}
				],
				"DayOff": null
			};

			var timelineVm = target.Create([scheduleForPerson1], '2018-10-17', 'ETC/UTC').TimeLine;

			expect(timelineVm.StartMinute).toEqual(480);
			expect(timelineVm.EndMinute).toEqual(1800);

			expect(timelineVm.HourPoints.length).toBe(23);

			expect(timelineVm.HourPoints[0].TimeLabel).toEqual('8:00 AM');
			expect(timelineVm.HourPoints[0].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[1].TimeLabel).toEqual('9:00 AM');
			expect(timelineVm.HourPoints[1].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[2].TimeLabel).toEqual('10:00 AM');
			expect(timelineVm.HourPoints[2].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[3].TimeLabel).toEqual('11:00 AM');
			expect(timelineVm.HourPoints[3].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[4].TimeLabel).toEqual('12:00 PM');
			expect(timelineVm.HourPoints[4].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[5].TimeLabel).toEqual('1:00 PM');
			expect(timelineVm.HourPoints[5].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[6].TimeLabel).toEqual('2:00 PM');
			expect(timelineVm.HourPoints[6].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[7].TimeLabel).toEqual('3:00 PM');
			expect(timelineVm.HourPoints[7].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[8].TimeLabel).toEqual('4:00 PM');
			expect(timelineVm.HourPoints[8].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[9].TimeLabel).toEqual('5:00 PM');
			expect(timelineVm.HourPoints[9].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[10].TimeLabel).toEqual('6:00 PM');
			expect(timelineVm.HourPoints[10].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[11].TimeLabel).toEqual('7:00 PM');
			expect(timelineVm.HourPoints[11].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[12].TimeLabel).toEqual('8:00 PM');
			expect(timelineVm.HourPoints[12].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[13].TimeLabel).toEqual('9:00 PM');
			expect(timelineVm.HourPoints[13].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[14].TimeLabel).toEqual('10:00 PM');
			expect(timelineVm.HourPoints[14].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[15].TimeLabel).toEqual('11:00 PM');
			expect(timelineVm.HourPoints[15].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[16].TimeLabel).toEqual('12:00 AM +1');
			expect(timelineVm.HourPoints[16].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[17].TimeLabel).toEqual('1:00 AM +1');
			expect(timelineVm.HourPoints[17].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[18].TimeLabel).toEqual('2:00 AM +1');
			expect(timelineVm.HourPoints[18].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[19].TimeLabel).toEqual('3:00 AM +1');
			expect(timelineVm.HourPoints[19].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[20].TimeLabel).toEqual('4:00 AM +1');
			expect(timelineVm.HourPoints[20].IsLabelVisible).toEqual(true);

			expect(timelineVm.HourPoints[21].TimeLabel).toEqual('5:00 AM +1');
			expect(timelineVm.HourPoints[21].IsLabelVisible).toEqual(false);

			expect(timelineVm.HourPoints[22].TimeLabel).toEqual('6:00 AM +1');
			expect(timelineVm.HourPoints[22].IsLabelVisible).toEqual(true);
		});

		it("should display closest earlier hour line for start and later hour for end when they are not at hour point", function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 07:05",
						"EndInUtc": "2018-10-17 09:00",
						"IsOvertime": false
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-17 09:00",
						"EndInUtc": "2018-10-17 10:00"
					}
				],
				"DayOff": null
			};

			var scheduleForPerson2 = {
				"PersonId": "person2",
				"Name": "person2",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 12:00",
						"EndInUtc": "2018-10-17 16:00"
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-17 16:00",
						"EndInUtc": "2018-10-17 20:05"
					}
				],
				"DayOff": null
			};

			var timeLine = target.Create([scheduleForPerson1, scheduleForPerson2], '2018-10-17', 'ETC/UTC').TimeLine;
			expect(timeLine.HourPoints.length).toEqual(15);

			var firstHourPoint = timeLine.HourPoints[0];
			expect(firstHourPoint.TimeLabel).toEqual("7:00 AM");
			expect(firstHourPoint.Position()).toEqual(0);

			var lastHourPoint = timeLine.HourPoints[14];
			expect(lastHourPoint.TimeLabel).toEqual("9:00 PM");
			expect(lastHourPoint.Position()).toEqual(840 * timeLine.LengthPercentPerMinute);
		});

		it('should get correct time label on start date of DST', function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-03-24',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-03-24 22:00",
						"EndInUtc": "2018-03-25 04:00",
						"IsOvertime": false
					}
				],
				"DayOff": null
			};

			var timelineVm = target.Create([scheduleForPerson1], '2018-03-25', 'Europe/Berlin').TimeLine;
			expect(timelineVm.HourPoints.length).toEqual(7);

			expect(timelineVm.HourPoints[0].TimeLabel).toEqual('12:00 AM');
			expect(timelineVm.HourPoints[1].TimeLabel).toEqual('1:00 AM');
			expect(timelineVm.HourPoints[2].TimeLabel).toEqual('3:00 AM');
			expect(timelineVm.HourPoints[3].TimeLabel).toEqual('4:00 AM');
			expect(timelineVm.HourPoints[4].TimeLabel).toEqual('5:00 AM');
			expect(timelineVm.HourPoints[5].TimeLabel).toEqual('6:00 AM');
			expect(timelineVm.HourPoints[6].TimeLabel).toEqual('7:00 AM');
		});

		it('should get correct time label on end date of DST', function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-28',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-27 22:00",
						"EndInUtc": "2018-10-28 04:00",
						"IsOvertime": false
					}
				],
				"DayOff": null
			};

			var timelineVm = target.Create([scheduleForPerson1], '2018-10-28', 'Europe/London').TimeLine;
			expect(timelineVm.HourPoints.length).toEqual(7);

			expect(timelineVm.HourPoints[0].TimeLabel).toEqual('12:00 AM');
			expect(timelineVm.HourPoints[1].TimeLabel).toEqual('1:00 AM');
			expect(timelineVm.HourPoints[2].TimeLabel).toEqual('1:00 AM');
			expect(timelineVm.HourPoints[3].TimeLabel).toEqual('2:00 AM');
			expect(timelineVm.HourPoints[4].TimeLabel).toEqual('3:00 AM');
			expect(timelineVm.HourPoints[5].TimeLabel).toEqual('4:00 AM');
			expect(timelineVm.HourPoints[6].TimeLabel).toEqual('5:00 AM');
		});
	});

	describe('in locale zh-CN', function () {
		beforeAll(function () {
			moment.locale('zh-CN');
		});

		afterAll(function () {
			moment.locale('en');
		});

		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				$provide.service('CurrentUserInfo', function () {
					return {
						CurrentUserInfo: function () {
							return {
								DefaultTimeZone: 'Asia/Hong_Kong',
								DateFormatLocale: 'zh-CN',
								FirstDayOfWeek: 1,
								DateTimeFormat: {
									ShowMeridian: false,
									ShortTimePattern: 'HH:mm',
									AMDesignator: '上午',
									PMDesignator: '下午'
								}

							};
						}
					};
				});
			});
		});

		beforeEach(inject(function (GroupScheduleFactory) {
			target = GroupScheduleFactory;
		}));

		it('should get correct formatted underlying schedule timespan', function () {
			var schedule = {
				"PersonId": "221B-Baker-Street",
				"Name": "Sherlock Holmes",
				"Date": "2018-10-16",
				"ContractTimeMinutes": 480,
				"Projection": [
					{
						"ShiftLayerIds": ["222"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-16 07:00",
						"EndInUtc": "2018-10-16 11:00"
					},
					{
						"ShiftLayerIds": ["333"],
						"Color": "#80FF80",
						"Description": "Email",
						"StartInUtc": "2018-10-16 11:00",
						"End": "2018-10-16 16:00"
					}],
				"DayOff": null,
				"UnderlyingScheduleSummary": {
					"PersonalActivities": [{
						"Description": "personal activity",
						"StartInUtc": '2018-10-16 10:00',
						"EndInUtc": '2018-10-16 11:00'
					}],
					"PersonPartTimeAbsences": [{
						"Description": "holiday",
						"StartInUtc": '2018-10-16 11:30',
						"EndInUtc": '2018-10-16 12:00'
					}],
					"PersonMeetings": [{
						"Description": "administration",
						"StartInUtc": '2018-10-16 14:00',
						"EndInUtc": '2018-10-16 15:00'
					}]
				}
			};
			var personScheduleVm = target.Create([schedule], '2018-10-16', 'ETC/UTC').Schedules[0];
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonalActivities[0].TimeSpan).toEqual("10:00 - 11:00");
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonPartTimeAbsences[0].TimeSpan).toEqual("11:30 - 12:00");
			expect(personScheduleVm.UnderlyingScheduleSummary.PersonMeetings[0].TimeSpan).toEqual("14:00 - 15:00");
		});

		it("should display 1 extra hour line when schedule starts or end at hour point", function () {
			var scheduleForPerson1 = {
				"PersonId": "person1",
				"Name": "person1",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 08:00",
						"EndInUtc": "2018-10-17 09:00",
						"IsOvertime": false
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-17 09:00",
						"EndInUtc": "2018-10-17 10:00"
					}
				],
				"DayOff": null
			};

			var scheduleForPerson2 = {
				"PersonId": "person2",
				"Name": "person2",
				"Date": '2018-10-17',
				"Projection": [
					{
						"ShiftLayerIds": ["31ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#795548",
						"Description": "Phone",
						"StartInUtc": "2018-10-17 12:00",
						"EndInUtc": "2018-10-17 16:00"
					},
					{
						"ShiftLayerIds": ["41ffe214-3384-4a80-a14c-a83800e23276"],
						"Color": "#FFFFFF",
						"Description": "Email",
						"StartInUtc": "2018-10-17 16:00",
						"EndInUtc": "2018-10-17 20:00"
					}
				],
				"DayOff": null
			};

			var timelineVm = target.Create([scheduleForPerson1, scheduleForPerson2], '2018-10-17', 'ETC/UTC').TimeLine;

			expect(timelineVm.HourPoints.length).toEqual(15);

			var firstHourPoint = timelineVm.HourPoints[0];
			expect(firstHourPoint.TimeLabel).toEqual("07:00");
			expect(firstHourPoint.Position()).toEqual(0);

			var lastHourPoint = timelineVm.HourPoints[timelineVm.HourPoints.length - 1];
			expect(lastHourPoint.TimeLabel).toEqual("21:00");
			expect(lastHourPoint.Position()).toEqual(840 * timelineVm.LengthPercentPerMinute);
		});
	});
})();