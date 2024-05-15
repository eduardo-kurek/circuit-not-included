using System;
using System.Collections.Generic;
using mod_oni.Base;

namespace mod_oni.Builders
{
	using InputPorts = Dictionary<HashedString, HashSet<HashedString>>;
	using OutputPorts = Dictionary<HashedString, Circuit.GetResult>;
	
	public abstract class Builder
	{
		public abstract Type Build();
	}
}