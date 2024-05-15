using System;

namespace mod_oni.Exceptions
{
	public class ArgumentException : Exception
	{
		public ArgumentException(string message, object param) 
			: base($"[{Debug.ModAbbr}] {message}; Parameter: {nameof(param)}"){ }
	}
}