
//TODO: tenant - should be removed
Teleopti.Start.Authentication.DataSourceViewModel = function (name, type) {
	this.Name = name;
	this.Type = type;
	this.IsWindows = (type == 'windows');
};
