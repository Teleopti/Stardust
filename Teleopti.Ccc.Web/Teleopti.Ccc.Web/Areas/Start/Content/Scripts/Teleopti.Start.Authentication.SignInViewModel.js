
Teleopti.Start.Authentication.SignInViewModel = function (data) {
	var self = this;

	this.DataSources = ko.observableArray();
	this.UserName = ko.observable('');
	this.Password = ko.observable('');
	this.ErrorMessage = ko.observable('');

	this.Ajax = new Teleopti.Start.Authentication.JQueryAjaxViewModel();

	this.SelectedDataSource = ko.computed(function () {
		return ko.utils.arrayFirst(self.DataSources(), function (d) {
			return d.Selected();
		});
	});

	this.DisplayUserNameAndPasswordBoxes = ko.computed(function () {
		var selected = self.SelectedDataSource();
		if (selected)
			if (selected.Type == "application") {
				setTimeout(function () { self.UserNameFocus(true); }, 1);
				return true;
			}
		return false;
	});

	this.UserNameFocus = ko.observable(false);

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
			cache: false,
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
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 500) {
//					var response = $.parseJSON(jqXHR.responseText);
					$('#Exception-message').text(errorThrown);
					$('#Exception-div').show();
					return;
				}
			}
		});
	};

	this.SignIn = function () {
		var state = data.authenticationState;

		state.TryToSignIn({
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
