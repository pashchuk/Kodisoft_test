using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLibrary.ConvertTypes
{
	public class StringType
	{
		private string value;

		public StringType(string value)
		{
			this.value = value;
		}

		public static implicit operator string(StringType val)
		{
			return val.value;
		}

		public static implicit operator StringType(string val)
		{
			return new StringType(val);
		}
		object Transform(object param)
		{
			return ((StringType) param).value.ToUpper();
		}
	}
}
