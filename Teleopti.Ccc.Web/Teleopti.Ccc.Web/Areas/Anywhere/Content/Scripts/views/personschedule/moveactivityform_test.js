define(['buster', 'views/personschedule/moveactivityform'],
	function (buster, moveactivityform) {
		return function () {

			buster.testCase("person schedule move activity viewmodel", {
				"should fill data for moving acitity form": function () {
					var data = { PersonId: "personId", Date: "2014-06-06", OldStartTime: "12:00", ProjectionLength: "60"};
					var form = new moveactivityform();
					form.SetData(data);
					assert.equals(form.PersonId(), data.PersonId);
					assert.equals(form.ScheduleDate(), data.Date);
					assert.equals(form.OldStartTime(), data.OldStartTime);
					assert.equals(form.ProjectionLength(), data.ProjectionLength);
				}

			});

		};
	});