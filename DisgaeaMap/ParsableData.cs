using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Cobalt.IO;

namespace DisgaeaMap
{
	public abstract class ParsableData
	{
		protected Dictionary<string, object> originalValues = new Dictionary<string, object>();

		ParsableData() { }

		protected ParsableData(Stream stream, Endian endianness = Endian.BigEndian)
		{
			ReadFromStream(stream, endianness);

			if (originalValues.Count == 0)
				GetOriginalValues();
		}

		public virtual void ReadFromStream(Stream stream, Endian endianness)
		{
			GetOriginalValues();
		}

		public virtual void WriteToStream(Stream stream, Endian endianness)
		{
			GetOriginalValues();
		}

		protected void GetOriginalValues()
		{
			originalValues = new Dictionary<string, object>();
			foreach (PropertyInfo prop in this.GetType().GetProperties().Where(x => x.CanWrite)) originalValues.Add(prop.Name, prop.GetValue(this, null));
		}

		public bool HasPropertyChanged(string property)
		{
			object value = GetType().GetProperty(property).GetValue(this, null);
			return (!value.Equals(originalValues[property]));
		}
	}
}
