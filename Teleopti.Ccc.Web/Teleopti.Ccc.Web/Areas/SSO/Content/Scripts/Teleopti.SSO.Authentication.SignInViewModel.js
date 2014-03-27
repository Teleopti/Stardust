
Teleopti.SSO.Authentication.SignInViewModel = function (data) {
	var self = this;

	this.DataSources = ko.observableArray();
	this.UserName = ko.observable('');
	this.Password = ko.observable('');
	this.ErrorMessage = ko.observable();
	this.HasErrorMessage = ko.computed(function () {
		var errorMessage = self.ErrorMessage();
		return errorMessage && errorMessage.length > 0;
	});

	this.Ajax = new Teleopti.SSO.Authentication.JQueryAjaxViewModel();

	this.SelectedDataSource = ko.observable();

	this.DisplayUserNameAndPasswordBoxes = ko.observable(false);

	this.UserNameFocus = ko.observable(false);

	this.HasDataSources = ko.computed(function() {
		return self.DataSources().length > 0;
	});

	this.SelectDataSource = function (dataSource) {
		self.SelectedDataSource(dataSource);

		if (!dataSource.IsWindows) {
			self.DisplayUserNameAndPasswordBoxes(true);
			self.UserNameFocus(false);
			self.UserNameFocus(true);
		} else {
			self.DisplayUserNameAndPasswordBoxes(false);
			self.UserNameFocus(false);
		}
		self.ErrorMessage('');
	};

	this.LoadDataSources = function () {
		$.ajax({
			url: data.baseUrl + "SSO/AuthenticationApi/DataSources",
			dataType: "json",
			type: 'GET',
			cache: false,
			success: function (data, textStatus, jqXHR) {
				self.DataSources([]);

				var map = ko.utils.arrayMap(data, function (d) {
					return new Teleopti.SSO.Authentication.DataSourceViewModel(d.Name, d.Type);
				});
				self.DataSources.push.apply(self.DataSources, map);
				var dataSources = self.DataSources();
				if (dataSources.length == 0) {
					self.ErrorMessage($('#Signin-error').data('nodatasourcetext'));
					return;
				}

				self.SelectDataSource(dataSources[0]);
				
			},
			error: function (jqXHR, textStatus, errorThrown) {
				try {
					var json = JSON.parse(jqXHR.responseText);
					$('#Exception-message').text(json.Message);
					$('#Exception-div').show();
				}
				catch (e) {
					$('#body-inner').html('<h2>Error: ' + jqXHR.status + '</h2>');
					$('#dialog-modal-header').text(errorThrown);
					$('#dialog-modal-body').html(jqXHR.responseText);
					$('#dialog-modal').modal('show');
				}
				
			}
		});
	};

	this.SignIn = function () {
		var state = data.authenticationState;

		self.ErrorMessage('');

		var selectedDataSource = self.SelectedDataSource();
		state.TryToSignIn({
			data: {
				type: "application",
				datasource: selectedDataSource.Name,
				username: self.UserName(),
				password: self.Password()
			},
			errormessage: function (message) {
				self.ErrorMessage(message);
			}
		});

	};

};
