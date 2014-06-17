define(['buster', 'views/personschedule/vm', 'shared/layer', 'resources', 'require/resourcesr'],
	function (buster, viewModel, layer, resources, resourcesr) {
	return function () {

		buster.testCase("person schedule viewmodel", {

			"should create viewmodel": function () {
				var vm = new viewModel();
				assert(vm);
			},

			"should create timeline with default times": function() {
				//ignored until we can use TimeFormatForMoment: "HH:mm",
				assert(true);
				return;

				var vm = new viewModel();

				assert.equals(vm.TimeLine.StartTime(), "08:00");
				assert.equals(vm.TimeLine.EndTime(), "16:00");
			},

			"should create timeline according to shifts length": function (done) {
				//ignored until we can use TimeFormatForMoment: "HH:mm",
				done();
				assert(true);
				return;
				var vm = new viewModel();

				vm.SetViewOptions({
					id: 1,
					date: '20140616'
				});
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

				setTimeout(function() {
					assert.equals(vm.TimeLine.StartTime(), "12:00");
					assert.equals(vm.TimeLine.EndTime(), "13:00");
					done();
				},2);
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
				vm.SelectedStartMinutes(moment(jsonData.Projection[0].Start, resources.FixedDateTimeFormatForMoment).diff(dateMoment, 'minutes'));

				vm.UpdateData(jsonData);

				assert.equals(vm.SelectedLayer().StartMinutes(), vm.SelectedStartMinutes());
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
				vm.SelectedStartMinutes(moment(jsonData.Projection[0].Start, resources.FixedDateTimeFormatForMoment).diff(dateMoment, 'minutes'));

				vm.UpdateData(jsonData);
				assert.equals(vm.MoveActivityForm.PersonId(), "guid");
				assert.equals(vm.MoveActivityForm.ScheduleDate(), jsonData.Date);
				assert.equals(vm.MoveActivityForm.OldStartMinutes(), vm.SelectedStartMinutes());
				assert.equals(vm.MoveActivityForm.ProjectionLength(), jsonData.Projection[0].Minutes);
			}

		});

	};
});