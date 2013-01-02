
Teleopti.Start.Authentication.SignInViewModel = function (data) {
	var self = this;

	this.DataSources = ko.observableArray();
	this.UserName = ko.observable('');
	this.Password = ko.observable('');
	this.ErrorMessage = ko.observable('');

	this.Ajax = new Teleopti.Start.Authentication.JQueryAjaxViewModel();

	this.SelectedDataSource = ko.observable();

	this.DisplayUserNameAndPasswordBoxes = ko.observable(false);

	this.UserNameFocus = ko.observable(false);

	this.SelectDataSource = function (dataSource) {
		self.SelectedDataSource(dataSource);

		if (dataSource.Type == "application") {
			self.DisplayUserNameAndPasswordBoxes(true);
			setTimeout(function() { self.UserNameFocus(true); }, 1);
		} else {
			self.DisplayUserNameAndPasswordBoxes(false);
			self.UserNameFocus(false);
		}
	};

	this.LoadDataSources = function () {
		$.ajax({
			url: data.baseUrl + "Start/AuthenticationApi/DataSources",
			dataType: "json",
			type: 'GET',
			cache: false,
			success: function (data, textStatus, jqXHR) {
				self.DataSources.removeAll();

				var map = ko.utils.arrayMap(data, function (d) {
					return new Teleopti.Start.Authentication.DataSourceViewModel(d.Name, d.Type);
				});
				self.DataSources.push.apply(self.DataSources, map);
				self.SelectDataSource(self.DataSources()[0]);
				$('#DataSources').show();
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 500) {
					$('#Exception-message').text(errorThrown);
					$('#Exception-div').show();
					return;
				}
			}
		});
	};

	this.SignIn = function () {
		var state = data.authenticationState;

		self.ErrorMessage('');
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
