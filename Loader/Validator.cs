using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
// ReSharper disable All

namespace mod_oni
{
	public class Validator
	{
		private object value;
		private string name;

		public Validator Set(string name, object value){
			this.value = value;
			this.name = name;
			return this;
		}
		
		public Validator Required(){
			if(this.value != null) return this;
			throw new Exception($"The field {this.name} is required ({this.value})");
		}
		
		public Validator NotEmptyString(){
			if(this.value is string && (string)this.value != "") return this;
			throw new Exception($"The field {this.name} cannot be empty string");
		}

		public Validator Int(){
			if(this.value is int) return this;
			throw new Exception($"The field {this.name} must be an integer");
		}

		public Validator Min(int min){
			if((int)this.value >= min) return this;
			throw new Exception($"The field {this.name} cannot be lower than {min}");
		}
		
		
		public Validator Max(int max){
			if((int)this.value <= max) return this;
			throw new Exception($"The field {this.name} cannot be higher than {max}");
		}

		public Validator Array(){
			if(this.value is IList) return this;
			throw new Exception($"The field {this.name} must be an array");
		}
		
	}
}