define(['buster', 'views/newpersonschedule/absencelistitem', 'lazy', 'shared/timezone-current','resources'],
	function (buster, viewModel, lazy, timezoneCurrent, resources) {

		timezoneCurrent.SetIanaTimeZone('Europe/Berlin');

		return function () {
			buster.testCase("person schedule absence list viewmodel", {
				"should create viewmodel": function() {
					var data = {
						StartTime: "2014-12-03 08:30",
						EndTime: "2014-12-03 08:45"
					}

					var vm = new viewModel(data);
					assert(vm);
				},

				"ensure intraday absence can be shortend": function () {
					var data = {
						StartTime: "2014-12-03 08:30",
						EndTime: "2014-12-03 08:45"
					}
					var vm = new viewModel(data);
					vm.CurrentDateMoment(moment("2014-12-03"));
					assert(vm.ValidateEndTimeIsAfterStartTime() && vm.ValidateEndTimeIsBeforeOriginalEndTime());
				},

				"ensure fullday absence can be shortend": function () {
					var data = {
						StartTime: "2014-11-03 08:30",
						EndTime: "2014-12-03 08:30"
					}
					var vm = new viewModel(data);
					vm.CurrentDateMoment(moment("2014-12-01"));
					assert(vm.ValidateEndTimeIsAfterStartTime() && vm.ValidateEndTimeIsBeforeOriginalEndTime());
				},

				"should be invalid when back to work time is earlier than the current absence start time": function () {
					var data = {
						StartTime: "2014-12-03 08:30",
						EndTime: "2014-12-03 08:45"
					}

					var vm = new viewModel(data);
					vm.CurrentDateMoment(moment("2014-12-02"));
					assert(!vm.ValidateEndTimeIsAfterStartTime());
				},
				
				"should display appropriate error when back to work date/time is later than the current absence end date/time": function () {
					var data = {
						StartTime: "2014-12-03 08:30",
						EndTime: "2014-12-03 08:45"
					}

					var vm = new viewModel(data);
					vm.EndDate("2014-12-03");
					vm.EndTime("08:50");
					assert(!vm.ValidateEndTimeIsBeforeOriginalEndTime());
				},

				"ensure fullday absence is validated appropriately": function () {
					var data = {
						StartTime: "2014-12-03 08:30",
						EndTime: "2014-11-01 08:30"
					}
					var vm = new viewModel(data);
					vm.CurrentDateMoment(moment("2014-12-01"));
					assert(!vm.ValidateEndTimeIsAfterStartTime());
				},

				"ensure back to work time is available in other timezone" : function() {

					var data = {
						StartTime: "2014-12-01 08:30",
						EndTime: "2014-12-03 08:30",
						IanaTimeZoneOther: "Europe/Istanbul" //1 hours difference
					}

					var vm = new viewModel(data);
					vm.EndTimeForAbsenceModify(moment("2014-12-02 8:29"));

					//DateTimeFormatForMoment: "YYYY-MM-DD HH:mm"
					assert(vm.ModifiedEndTimeOtherTimeZone()=="2014-12-02 09:29");
				}

				
			});
		}
	}
);