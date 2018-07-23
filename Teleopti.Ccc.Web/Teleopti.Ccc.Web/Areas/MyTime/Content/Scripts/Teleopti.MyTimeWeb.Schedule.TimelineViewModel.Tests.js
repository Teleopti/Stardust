$(document).ready(function() {
	var fakeTimeLineRawData = [];

	module('Teleopti.MyTimeWeb.Schedule.TimelineViewModel', {
		setup: function() {
			setFakeTimeLineData();
		},
		teardown: function() {}
	});

	test('should calculate minute of time line view model correctly when not using percentage', function() {
		var timelineViewModel = [];

		fakeTimeLineRawData.forEach(function(t, i) {
			timelineViewModel.push(new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(t, 800, 50, false, i + 1));
		});

		equal(timelineViewModel[0].minutes, 360);
		equal(timelineViewModel[21].minutes, 1620);
	});

	test('should support timeline with date time', function() {
		var timelineViewModelList = [];

		fakeTimeLineRawData.forEach(function(t, i) {
			var time = moment().format('YYYY-MM-DD') + 'T' + t.Time;
			var timeDisplay = moment().format('YYYY-MM-DD') + 'T' + t.TimeLineDisplay;
			if (t.Time.indexOf('.') > -1) {
				time =
					moment()
						.add(1, 'days')
						.format('YYYY-MM-DD') +
					'T' +
					t.Time;

				timeDisplay =
					moment()
						.add(1, 'days')
						.format('YYYY-MM-DD') +
					'T' +
					t.TimeLineDisplay;
			}

			var timelineViewModel = new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(
				{
					Time: time,
					TimeLineDisplay: moment().format('YYYY-MM-DD') + 'T' + t.TimeLineDisplay,
					PositionPercentage: null,
					TimeFixedFormat: null
				},
				800,
				50,
				false,
				i + 1
			);

			timelineViewModelList.push(timelineViewModel);
		});

		equal(timelineViewModelList[0].timeText, '06:00');
		equal(timelineViewModelList[21].timeText, '03:00 +1');
	});

	function setFakeTimeLineData() {
		fakeTimeLineRawData = [
			{
				Time: '06:00:00',
				TimeLineDisplay: '06:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '07:00:00',
				TimeLineDisplay: '07:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '08:00:00',
				TimeLineDisplay: '08:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '09:00:00',
				TimeLineDisplay: '09:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '10:00:00',
				TimeLineDisplay: '10:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '11:00:00',
				TimeLineDisplay: '11:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '12:00:00',
				TimeLineDisplay: '12:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '13:00:00',
				TimeLineDisplay: '13:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '14:00:00',
				TimeLineDisplay: '14:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '15:00:00',
				TimeLineDisplay: '15:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '16:00:00',
				TimeLineDisplay: '16:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '17:00:00',
				TimeLineDisplay: '17:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '18:00:00',
				TimeLineDisplay: '18:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '19:00:00',
				TimeLineDisplay: '19:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '20:00:00',
				TimeLineDisplay: '20:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '21:00:00',
				TimeLineDisplay: '21:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '22:00:00',
				TimeLineDisplay: '22:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '23:00:00',
				TimeLineDisplay: '23:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '1.00:00:00',
				TimeLineDisplay: '00:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '1.01:00:00',
				TimeLineDisplay: '01:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '1.02:00:00',
				TimeLineDisplay: '02:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			},
			{
				Time: '1.03:00:00',
				TimeLineDisplay: '03:00',
				PositionPercentage: null,
				TimeFixedFormat: null
			}
		];
	}
});
