import { Adina, adina, eva, myles } from './people';

export type AdinaLogon = {
	PersonId: typeof adina.Id;
	LogonName: 'adina';
	Identity: 'ORG/adina';
};
export const adinaLogon: AdinaLogon = {
	PersonId: adina.Id,
	LogonName: 'adina',
	Identity: 'ORG/adina'
};
export type EvaLogon = {
	PersonId: typeof eva.Id;
	LogonName: 'evv';
	Identity: 'ORG/eva';
};
export const evaLogon: EvaLogon = {
	PersonId: eva.Id,
	LogonName: 'evv',
	Identity: 'ORG/eva'
};
export type MylesLogon = {
	PersonId: typeof myles.Id;
	LogonName: '';
	Identity: '';
};
export const mylesLogon: MylesLogon = {
	PersonId: myles.Id,
	LogonName: '',
	Identity: ''
};
