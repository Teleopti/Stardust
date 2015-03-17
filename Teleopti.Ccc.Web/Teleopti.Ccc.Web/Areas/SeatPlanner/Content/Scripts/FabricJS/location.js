define([
	'fabric'
], function (
	fabric
	) {

	// Location definition
	fabric.Location = fabric.util.createClass(fabric.Rect, {
		type: 'location',
		initialize: function ( options) {

			options || (options = {});

			this.callSuper('initialize', options);
			this.set('id', options.id);
			this.set('name', options.name)
		},

		toObject: function () {
			return fabric.util.object.extend(this.callSuper('toObject'), {
				id: this.get('id'),
				name: this.get('name')
			});
		},

		_render: function (ctx) {
			this.callSuper('_render', ctx);

			ctx.font = '18px Helvetica';
			ctx.fillStyle = '#333';
			ctx.fillText(this.get('name'), 30 -this.width / 2, 30 -this.height / 2 );
			
		}
		
	});

	fabric.Location.async = true;

	//fabric.Location.fromObject = function (object, callback) {
	//	fabric.util.loadImage(object.src, function (img) {
	//		callback && callback(new fabric.Location(img, object));
	//	});
	//};

	fabric.Location.fromObject = function (object, callback) {
				callback && callback(new fabric.Location(object));		
		};


});
