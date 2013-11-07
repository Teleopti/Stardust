define([
		'knockout',
		'views/teamschedule/group-page-group'
	], function (
		ko,
		group
	) {

		return function (data) {

			var self = this;
			
			this.Name = data.Name;
			
			this.Groups = ko.observableArray();
			
			var newItems = ko.utils.arrayMap(data.Groups, function (d) {
				return new group(d);
			});
			self.Groups.push.apply(self.Groups, newItems);
		};
	});
