define([
		'knockout'
	], function (ko) {

	    return function (timeline, start, length, color, percentage, lowestSkill, highestSkill) {

	        var self = this;

	        this.StartMinutes = start;
	        this.LengthMinutes = length;
	        this.Color = ko.observable(color);
	        this.Percentage = ko.observable(percentage);
	        this.LowestSkill = ko.observable(lowestSkill);
	        this.HighestSkill = ko.observable(highestSkill);

	        this.Title = ko.computed(function () {
	            return self.Percentage()*100 + '%<br />Worst: ' + self.LowestSkill() + '<br />Best: ' + self.HighestSkill();
	        });

	        this.StartPixels = ko.computed(function () {
	            var startMinutes = self.StartMinutes - timeline.StartMinutes();
	            var pixels = startMinutes * timeline.PixelsPerMinute();
	            return Math.round(pixels);
	        });

	        this.LengthPixels = ko.computed(function () {
	            var lengthMinutes = self.LengthMinutes;
	            var pixels = lengthMinutes * timeline.PixelsPerMinute();
	            return Math.round(pixels);
	        });
	    };
	});
