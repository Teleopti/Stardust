
define([
		'knockout',
		'jquery',
		'helpers',
		'navigation',
		'views/teamschedule/fakeData',
		'views/teamschedule/vm',
		'views/teamschedule/timeline',
		'views/teamschedule/resources',
		'text!templates/teamschedule/view.html'
	], function (
		ko,
		$,
		helpers,
		navigation,
		fakeData,
		teamScheduleViewModel, timeLineViewModel, resourcesViewModel,
		view
	) {
	    return {
			display: function (options) {
				
				options.renderHtml(view);

	            $('#date-selection').datepicker({
	                pullRight: true,
	                format: 'yyyy-mm-dd',
	                weekStart: 1
	            }).on('changeDate', function (ev) {
	                $('#date-selection').datepicker('hide');
	            });

	            var timeLine = new timeLineViewModel();

	            var resourceLayers = fakeData.GetResources(timeLine);
	            var resources = new resourcesViewModel("Resources", resourceLayers);

	            var teamSchedule = new teamScheduleViewModel(timeLine, resources);

	            var agents = fakeData.GetAgents(timeLine);
	            teamSchedule.AddAgents(agents);

	            var resize = function () {
	                timeLine.WidthPixels($('.shift').width());
	            };
	            $(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

	            ko.applyBindings({
	                TeamSchedule: teamSchedule
	            });

	            $('.agent').click(function () {
	                navigation.GotoAgentSchedule($(this).data('agent-id'), $('#date-selection').attr('value'));
	            });

	            $('.resources').click(function () {
	                navigation.GotoResources($('#date-selection').attr('value'));
	            });
			    
	            $('.resource-layer').tooltip();
	        }
	    };

	});

