
define([
		'helpers',
		'views/teamschedule/agent',
		'views/teamschedule/layer',
		'views/teamschedule/resourceLayer'
	], function (
		helpers,
		agentViewModel,
		layerViewModel,
		resourceLayerViewModel
	) {

	    var minutes = helpers.Minutes;

	    function _getResources(timeLine) {
	        var green = "#55bb55";
	        var yellow = "#ffff55";

	        return [
	            new resourceLayerViewModel(timeLine, 60, 15, yellow, 0.7, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 75, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 90, 15, yellow, 0.72, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 105, 15, green, 0.73, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 120, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 135, 15, green, 0.73, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 150, 15, green, 0.74, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 165, 15, green, 0.74, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 180, 15, green, 0.76, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 195, 15, green, 0.77, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 210, 15, yellow, 0.70, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 225, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 240, 15, yellow, 0.72, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 255, 15, yellow, 0.72, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 270, 15, yellow, 0.72, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 285, 15, green, 0.74, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 300, 15, green, 0.75, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 315, 15, green, 0.75, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 330, 15, green, 0.75, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 345, 15, green, 0.76, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 360, 15, green, 0.78, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 375, 15, green, 0.79, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 390, 15, green, 0.79, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 405, 15, green, 0.74, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 420, 15, green, 0.73, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 435, 15, green, 0.73, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 450, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 465, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 480, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 495, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 510, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 525, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 540, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 555, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 570, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 585, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 600, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 615, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 630, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 645, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 660, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 675, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 690, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 705, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 720, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 735, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 750, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 765, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 780, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 795, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 810, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 825, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 840, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 855, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 870, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 885, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 900, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 915, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 930, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 945, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 960, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 975, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 990, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1005, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1020, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1035, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1050, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1065, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1080, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1095, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1110, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1125, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1140, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1155, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1170, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)'),
	            new resourceLayerViewModel(timeLine, 1185, 15, yellow, 0.71, 'Store1 (65.6%)', 'Direct Sales (78.4%)')
	        ];
	    }

	    function _getAgents(timeLine) {

	        var green = "#55bb55";
	        var yellow = "#ffff55";

	        return [

					new agentViewModel(
						"Ashley Andeen",
						[
							new layerViewModel(timeLine, minutes.FromHours(3.5), minutes.FromHours(2.5), green),
							new layerViewModel(timeLine, minutes.FromHours(6), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(7), minutes.FromHours(3), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Prashant Arora",
						[
							new layerViewModel(timeLine, minutes.FromHours(5), minutes.FromHours(4), green),
							new layerViewModel(timeLine, minutes.FromHours(9), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(10), minutes.FromHours(2.5), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Pierre Baldi",
						[
							new layerViewModel(timeLine, minutes.FromHours(2), minutes.FromHours(3), green),
							new layerViewModel(timeLine, minutes.FromHours(5), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(6), minutes.FromHours(2), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Bill Gates",
						[
							new layerViewModel(timeLine, minutes.FromHours(5), minutes.FromHours(3), green),
							new layerViewModel(timeLine, minutes.FromHours(7), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(8), minutes.FromHours(2), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Steve Jobs",
						[
							new layerViewModel(timeLine, minutes.FromHours(1.5), minutes.FromHours(5.5), green),
							new layerViewModel(timeLine, minutes.FromHours(6), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(7), minutes.FromHours(1), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Ward Cunningham",
						[
							new layerViewModel(timeLine, minutes.FromHours(8), minutes.FromHours(4), green),
							new layerViewModel(timeLine, minutes.FromHours(12), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(13), minutes.FromHours(3), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Kent Beck",
						[
							new layerViewModel(timeLine, minutes.FromHours(7), minutes.FromHours(4), green),
							new layerViewModel(timeLine, minutes.FromHours(11), minutes.FromHours(0.5), yellow),
							new layerViewModel(timeLine, minutes.FromHours(11.5), minutes.FromHours(6), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Roy Osherove",
						[
							new layerViewModel(timeLine, minutes.FromHours(9), minutes.FromHours(2), green),
							new layerViewModel(timeLine, minutes.FromHours(11), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(12), minutes.FromHours(5), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Carl Franklin",
						[
							new layerViewModel(timeLine, minutes.FromHours(12), minutes.FromHours(2), green),
							new layerViewModel(timeLine, minutes.FromHours(14), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(15), minutes.FromHours(3), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Richard Cambell",
						[
							new layerViewModel(timeLine, minutes.FromHours(9), minutes.FromHours(3), green),
							new layerViewModel(timeLine, minutes.FromHours(12), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(13), minutes.FromHours(2), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Scott Hanselman",
						[
							new layerViewModel(timeLine, minutes.FromHours(5.5), minutes.FromHours(3.5), green),
							new layerViewModel(timeLine, minutes.FromHours(9), minutes.FromHours(2), yellow),
							new layerViewModel(timeLine, minutes.FromHours(11), minutes.FromHours(3), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Martin Fowler",
						[
							new layerViewModel(timeLine, minutes.FromHours(5), minutes.FromHours(4), green),
							new layerViewModel(timeLine, minutes.FromHours(9), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(10), minutes.FromHours(4), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Mark Spencer",
						[
							new layerViewModel(timeLine, minutes.FromHours(3), minutes.FromHours(4), green),
							new layerViewModel(timeLine, minutes.FromHours(7), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(8), minutes.FromHours(4), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Mathias Stenbom",
						[
							new layerViewModel(timeLine, minutes.FromHours(8), minutes.FromHours(4), green),
							new layerViewModel(timeLine, minutes.FromHours(12), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(13), minutes.FromHours(4), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Robin Karlsson",
						[
							new layerViewModel(timeLine, minutes.FromHours(7), minutes.FromHours(3), green),
							new layerViewModel(timeLine, minutes.FromHours(10), minutes.FromHours(2), yellow),
							new layerViewModel(timeLine, minutes.FromHours(12), minutes.FromHours(5), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"David Jonsson",
						[
							new layerViewModel(timeLine, minutes.FromHours(5), minutes.FromHours(4), green),
							new layerViewModel(timeLine, minutes.FromHours(9), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(10), minutes.FromHours(4), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Kunning Mao",
						[
							new layerViewModel(timeLine, minutes.FromHours(9), minutes.FromHours(5), green),
							new layerViewModel(timeLine, minutes.FromHours(14), minutes.FromHours(0.5), yellow),
							new layerViewModel(timeLine, minutes.FromHours(14.5), minutes.FromHours(5), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Johan Ryding",
						[
							new layerViewModel(timeLine, minutes.FromHours(12), minutes.FromHours(3), green),
							new layerViewModel(timeLine, minutes.FromHours(15), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(16), minutes.FromHours(3), green)
						],
						"8:30",
						"8:30"
					),
					new agentViewModel(
						"Mathias Engblom",
						[
							new layerViewModel(timeLine, minutes.FromHours(7), minutes.FromHours(4), green),
							new layerViewModel(timeLine, minutes.FromHours(11), minutes.FromHours(1), yellow),
							new layerViewModel(timeLine, minutes.FromHours(12), minutes.FromHours(3.5), green)
						],
						"8:30",
						"8:30"
					)
				];
	    }

	    return {
	        GetAgents: function (timeLine) {
	            return _getAgents(timeLine);
	        },
	        GetResources: function (timeLine) {
	            return _getResources(timeLine);
	        }
	    };

	});

