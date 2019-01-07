
// Seat definition
fabric.Seat = fabric.util.createClass(fabric.Image, {
	type: 'seat',
	initialize: function (element, options) {

		options || (options = {});

		this.callSuper('initialize', element, options);
		this.set('id', options.id);
		this.set('name', options.name);
		this.set('priority', options.priority);
	},

	toObject: function () {
		return fabric.util.object.extend(this.callSuper('toObject'), {
			id: this.get('id')
		});
	},

	_render: function (ctx) {
		this.callSuper('_render', ctx);

		ctx.font = '15px Helvetica';
		ctx.fillStyle = '#333';
		var topmostGroup = this.getTopMostGroup(this);

		var angle = topmostGroup != null && topmostGroup.angle ? topmostGroup.angle + this.angle : this.angle;

		ctx.rotate(Math.PI / 180 * -angle);
		ctx.fillText(this.get('priority'), -this.width / 2, -this.height / 2);
		ctx.rotate(Math.PI / 180 * (angle));

	},

	getTopMostGroup: function (obj) {
		var group = null;
		if (obj.group != null) {
			var childGroup = this.getTopMostGroup(obj.group);
			group = childGroup || obj.group;
		}
		return group;
	}
});

fabric.Seat.async = true;

fabric.Seat.fromObject = function (object, callback) {
    object.src = 'dist/ng2/assets/seatMap/seat.svg';
	fabric.util.loadImage(object.src, function (img) {
		callback && callback(new fabric.Seat(img, object));
	});
};
