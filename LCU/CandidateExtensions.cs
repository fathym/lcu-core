using System;
using System.Collections.Generic;
using System.Text;

namespace LCU
{
	public static class CandidateExtensions
	{
		public static string SecondToLastToEnd(this string value, string lookup)
{
			return value.Substring(value.Substring(0, value.LastIndexOf(lookup)).LastIndexOf(lookup) + 1);
		}
	}
}
