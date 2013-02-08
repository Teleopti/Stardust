using System;
using System.Collections.Generic;
using AutoMapper;
using Rhino.Mocks;

namespace Teleopti.Ccc.WebTest
{
	/// <summary>
	/// Used for creating simple mappers for testing
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TU"></typeparam>
	public class MappingProfileForProbing<T, TU> : Profile
	{
		private readonly IList<T> _from = new List<T>();

		/// <summary>
		/// The result thats returned from the mapper
		/// </summary>
		public TU Result { get; set; }

		/// <summary>
		/// Verifies if the source has been mapped
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public bool HasBeenMappedFrom(T source)
		{
			return _from.Contains(source);
		}

		protected override void Configure()
		{
			CreateMap<T, TU>()
				.ConvertUsing(t =>
					              {
						              _from.Add(t);
						              return Result;
					              });
		}

	}
}