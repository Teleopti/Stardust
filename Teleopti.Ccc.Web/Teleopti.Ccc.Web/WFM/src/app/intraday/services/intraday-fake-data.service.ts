import { Injectable } from '@angular/core';
import moment, { Moment } from 'moment';
import { IntradayTrafficData, IntradayPerformanceData, Skill } from '../types';

@Injectable()
export class IntradayFakeDataService {
	private words = [
		'compensation',
		'embox',
		'means',
		'excavation',
		'greeting',
		'basin',
		'bag',
		'racism',
		'care',
		'boot',
		'survey',
		'particular',
		'grateful',
		'concession',
		'convulsion',
		'marble',
		'technique',
		'ribbon',
		'clinic',
		'idea',
		'miracle',
		'vague',
		'maze',
		'psychology',
		'illustrate',
		'wheel',
		'initiative',
		'mean',
		'context',
		'neglect',
		'flock',
		'adopt',
		'critic',
		'cut',
		'twilight',
		'storage',
		'sand',
		'beg',
		'extort',
		'curriculum',
		'density',
		'loot',
		'exemption',
		'silk',
		'resolution',
		'riot',
		'surprise',
		'clash',
		'portion',
		'half',
		'mobile',
		'compact',
		'dive',
		'acute',
		'nuance',
		'negotiation',
		'mind',
		'girl',
		'brilliance',
		'shine',
		'ruin',
		'empirical',
		'relevance',
		'affinity',
		'personal',
		'allowance',
		'estate',
		'couple',
		'start',
		'hostility',
		'sample',
		'monster',
		'energy',
		'salt',
		'wrong',
		'paragraph',
		'draft',
		'symbol',
		'execution',
		'education',
		'fist',
		'pattern',
		'excuse',
		'performer',
		'camp',
		'illusion',
		'triangle',
		'kettle',
		'crash',
		'trance',
		'ivory',
		'bet',
		'computing',
		'quiet',
		'fool around',
		'scenario',
		'tumble',
		'dollar',
		'bargain',
		'pie'
	];

	getRandom(min: number, max: number): number {
		return Math.random() * (max - min) + min;
	}

	getRandomInt(min: number, max: number): number {
		return Math.round(Math.random() * (max - min) + min);
	}

	getRandomSeries(min: number, max: number, count: number): number[] {
		const a: number[] = [];
		for (let i = 0; i < count; i++) {
			a.push(this.getRandom(min, max));
		}
		return a;
	}

	getNullSeries(count: number): number[] {
		const a: number[] = [];
		for (let i = 0; i < count; i++) {
			a.push(null);
		}
		return a;
	}

	getTimeSeries(start: Moment, incrementInMinutes: number, count: number): string[] {
		// Start at '2018-08-17T08:00:00'
		const theTime = moment(start);
		const series: string[] = [];
		for (let i = 0; i < count; i++) {
			series.push(theTime.format('YYYY-MM-DD[T]HH:mm:ss'));
			theTime.add(incrementInMinutes, 'minutes');
		}
		return series;
	}

	toMyFormat(time: Moment): string {
		return time.format('YYYY-MM-DDTHH:mm:ss');
	}

	getDummyTrafficData(startTimeString: Moment, timeIncrement: number = 15): IntradayTrafficData {
		const firstIntervalStart = moment(startTimeString);
		const firstIntervalEnd = moment(firstIntervalStart).add(15, 'minutes');
		const count = 27;
		return {
			FirstIntervalStart: this.toMyFormat(firstIntervalStart),
			FirstIntervalEnd: this.toMyFormat(firstIntervalEnd),
			LatestActualIntervalStart: null,
			LatestActualIntervalEnd: null,
			Summary: {
				ForecastedCalls: 0.0,
				ForecastedAverageHandleTime: 0.0,
				ForecastedHandleTime: 0.0,
				CalculatedCalls: 0.0,
				AverageHandleTime: 0.0,
				HandleTime: 0.0,
				ForecastedActualCallsDiff: -99.0,
				ForecastedActualHandleTimeDiff: -99.0,
				AverageSpeedOfAnswer: -99.0,
				SpeedOfAnswer: 0.0,
				AnsweredCalls: 0.0,
				ServiceLevel: -99.0,
				AnsweredCallsWithinSL: 0.0,
				AbandonRate: -99.0,
				AbandonedCalls: 0.0
			},
			DataSeries: {
				AverageSpeedOfAnswer: this.getNullSeries(count),
				Time: this.getTimeSeries(firstIntervalStart, 15, count),
				ForecastedCalls: this.getRandomSeries(0, 10, count),
				ForecastedAverageHandleTime: this.getRandomSeries(100, 200, count),
				AverageHandleTime: this.getNullSeries(count),
				CalculatedCalls: this.getNullSeries(count),
				AbandonedRate: this.getNullSeries(count),
				ServiceLevel: this.getNullSeries(count)
			},
			IncomingTrafficHasData: true
		};
	}

	getDummyPerformanceData(startTimeString: Moment, timeIncrement: number = 15): IntradayPerformanceData {
		const firstIntervalStart = moment(startTimeString);
		const firstIntervalEnd = moment(firstIntervalStart).add(15, 'minutes');
		const count = 27;
		return {
			DataSeries: {
				Time: this.getTimeSeries(firstIntervalStart, timeIncrement, count),
				EstimatedServiceLevels: this.getRandomSeries(10, 200, count),
				AverageSpeedOfAnswer: this.getRandomSeries(10, 200, count),
				AbandonedRate: this.getRandomSeries(0, 100, count),
				ServiceLevel: this.getRandomSeries(10, 200, count)
			},
			Summary: {
				AverageSpeedOfAnswer: -99.0,
				ServiceLevel: -99.0,
				AbandonRate: -99.0,
				EstimatedServiceLevel: 0.0
			},
			LatestActualIntervalStart: this.toMyFormat(firstIntervalStart),
			LatestActualIntervalEnd: this.toMyFormat(firstIntervalEnd),
			PerformanceHasData: true
		};
	}

	pickOneWord = (): string => this.words[this.getRandomInt(0, this.words.length - 1)];

	public generateSkills(count: number): Skill[] {
		const w: Skill[] = [];
		for (let i = 0; i < count; i++) {
			w.push({
				DoDisplayData: true,
				Id: this.getRandomInt(1, 10000) + '',
				IsMultisiteSkill: false,
				Name: this.pickOneWord(),
				ShowAbandonRate: true,
				ShowReforecastedAgent: true,
				SkillType: 'fake'
			});
		}

		return w;
	}
}
