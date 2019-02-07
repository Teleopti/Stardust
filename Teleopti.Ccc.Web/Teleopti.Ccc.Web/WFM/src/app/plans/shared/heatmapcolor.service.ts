import { Injectable } from '@angular/core';

@Injectable()
export class HeatMapColorHelper {
	min: number;
	max: number;
	gradients: Array<HeatMapColorValue>;
	constructor() {
		this.gradients = [
			new HeatMapColorValue(255, 0, 0, -100),  //red
			new HeatMapColorValue(255, 255, 255, 0),        //white
			new HeatMapColorValue(0, 0, 255, 100),    //blue
		];

		this.min = this.gradients[0].value;
		this.max = this.gradients[this.gradients.length -1].value;
	}

	getColor(value: number) {
		if (value < this.min) value = this.min;
		if (value > this.max) value = this.max;

		for (let i = 0; i < this.gradients.length; i++) {
			const gradientColor = this.gradients[i];
			if (!(value < gradientColor.value)) continue;
			const previousGradientColor = this.gradients[Math.max(0, i - 1)];
			const diff = previousGradientColor.value - gradientColor.value;
			let fraction = 0;
			if (diff !== 0)
				fraction = (value - gradientColor.value) / diff;
			const red = Math.round((previousGradientColor.red - gradientColor.red) * fraction + gradientColor.red);
			const green = Math.round((previousGradientColor.green - gradientColor.green) * fraction + gradientColor.green);
			const blue = Math.round((previousGradientColor.blue - gradientColor.blue) * fraction + gradientColor.blue);

			return `#${this.convertRgbToHex(red, green, blue)}`;
		}

		const last = this.gradients[this.gradients.length - 1];
		return `#${this.convertRgbToHex(last.red, last.green, last.blue)}`;
	}

	convertToHex(value: number) {
		let hex = Number(value).toString(16);
		if (hex.length < 2) {
			hex = `0${hex}`;
		}
		return hex;
	}

	convertRgbToHex(r: number, g: number, b: number) {
		const red = this.convertToHex(r);
		const green = this.convertToHex(g);
		const blue = this.convertToHex(b);
		return red + green + blue;
	}
}


class HeatMapColorValue {
	red: number;
	green: number;
	blue: number;
	value: number;

	constructor(r: number, g: number, b: number, v : number) {
		this.red = r;
		this.green = g;
		this.blue = b;
		this.value = v;
	}
}