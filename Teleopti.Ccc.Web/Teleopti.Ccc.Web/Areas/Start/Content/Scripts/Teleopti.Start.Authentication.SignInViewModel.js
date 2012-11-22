
Teleopti.Start.Authentication.SignInViewModel = function (data) {
	var self = this;

	this.DataSources = ko.observableArray();
	this.UserName = ko.observable();
	this.Password = ko.observable();
	this.ErrorMessage = ko.observable('');

	this.SelectedDataSource = ko.computed(function () {
		return ko.utils.arrayFirst(self.DataSources(), function (d) {
			return d.Selected();
		});
	});

	this.DisplayUserNameAndPasswordBoxes = ko.computed(function () {
		var selected = self.SelectedDataSource();
		if (selected)
			return selected.Type == "application";
		return false;
	});

	data.events.subscribe(function (dataSource) {
		ko.utils.arrayForEach(self.DataSources(), function (d) {
			d.Selected(d === dataSource);
		});
	}, null, "dataSourceSelected");

	this.LoadDataSources = function () {
		var events = data.events;
		$.ajax({
			url: data.baseUrl + "Start/AuthenticationApi/DataSources",
			dataType: "json",
			type: 'GET',
			success: function (data, textStatus, jqXHR) {
				self.DataSources.removeAll();
				for (var i = 0; i < data.length; i++) {
					var dataSource = new Teleopti.Start.Authentication.DataSourceViewModel({
						events: events
					});
					dataSource.Selected(i == 0);
					dataSource.Name = data[i].Name;
					dataSource.Type = data[i].Type;
					self.DataSources.push(dataSource);
				}
			}
		});
	};

	this.SignIn = function () {
		var state = data.authenticationState;

		state.AttemptGotoApplicationBySignIn({
			data: {
				type: self.SelectedDataSource().Type,
				datasource: self.SelectedDataSource().Name,
				username: self.UserName(),
				password: self.Password()
			},
			errormessage: function (message) {
				self.ErrorMessage(message);
			},
			nobusinessunit: function () {
				self.ErrorMessage($('#Signin-error').data('nobusinessunitext'));
			}
		});

	};

};
