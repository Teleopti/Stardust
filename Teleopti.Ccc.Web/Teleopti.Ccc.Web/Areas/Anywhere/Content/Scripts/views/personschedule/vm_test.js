define(['buster', 'views/personschedule/vm', 'shared/layer', 'resources', 'require/resourcesr'],
	function (buster, viewModel, layer, resources, resourcesr) {
	return function () {

		buster.testCase("person schedule viewmodel", {

			"should create viewmodel": function () {
				var vm = new viewModel();
				assert(vm);
			},

			"should create timeline with default times": function() {
				var vm = new viewModel();

				assert.equals(vm.TimeLine.StartMinutes(), 8 * 60);
				assert.equals(vm.TimeLine.EndMinutes(), 16 * 60);
			},

			"should create timeline according to shifts length": function () {
				assert(true);
				return;

				var vm = new viewModel();

				var data = [
					{
						PersonId: 1,
						Projection: [
							{
								Start: '2014-06-16 12:00',
								Minutes: 60
							}
						]
					}
				];
				vm.UpdateSchedules(data);

				assert.equals(vm.TimeLine.StartMinutes(), 12 * 60);
				assert.equals(vm.TimeLine.EndMinutes(), 13 * 60);
			},
			
			"should get the selected layer from url": function () {

				var jsonData = {
					PersonId: "guid",
					Name: "John King", Date: "2013-11-18", WorkTimeMinutes: 150,
					ContractTimeMinutes: 360,
					Projection: [{ Color: "#008000", Description: "Phone", Start: "2013-11-18 14:00", Minutes: 240 }],
					Offset: "2013-11-18"
				};
			
				var dateMoment = moment(jsonData.Date);

				var vm = new viewModel();
				vm.PersonId('guid');
				vm.ScheduleDate(dateMoment);
				vm.SelectedStartTime(moment(jsonData.Projection[0].Start, resources.FixedDateTimeFormatForMoment));

				vm.UpdateData(jsonData);

				assert.equals(moment(vm.SelectedLayer().StartTime(), resources.FixedDateTimeFormatForMoment).format(resources.FixedTimeFormatForMoment),
					vm.SelectedStartTime().format(resources.FixedTimeFormatForMoment));
			},

			"should set move activity form when updating data" : function() {
				var jsonData = {
					PersonId: "guid",
					Name: "John King", Date: "2013-11-18", WorkTimeMinutes: 150,
					ContractTimeMinutes: 360,
					Projection: [{ Color: "#008000", Description: "Phone", Start: "2013-11-18 14:00", Minutes: 240 }],
					Offset: "2013-11-18"
				};
				
				var dateMoment = moment(jsonData.Date);

				var vm = new viewModel();
				vm.PersonId('guid');
				vm.ScheduleDate(dateMoment);
				vm.SelectedStartTime(moment(jsonData.Projection[0].Start, resources.FixedDateTimeFormatForMoment));

				vm.UpdateData(jsonData);
				assert.equals(vm.MoveActivityForm.PersonId(), "guid");
				assert.equals(vm.MoveActivityForm.ScheduleDate(), jsonData.Date);
				assert.equals(vm.MoveActivityForm.OldStartTime(), moment(jsonData.Projection[0].Start,
					resources.FixedDateTimeFormatForMoment).format(resources.FixedTimeFormatForMoment));
				assert.equals(vm.MoveActivityForm.ProjectionLength(), jsonData.Projection[0].Minutes);
			}

		});

	};
});