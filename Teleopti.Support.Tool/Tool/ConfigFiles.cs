﻿using System;
using System.IO;

namespace Teleopti.Support.Tool.Tool
{
	public class ConfigFiles
	{
		private readonly string _file;

		public ConfigFiles(string file)
		{
			_file = file;
		}

		public string[] FileContents()
		{
			return File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ConfigFiles\" + _file));
		}
	}
}