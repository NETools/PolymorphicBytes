using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStream
{
	public interface IObjectSerializer
	{
		void Serialize(Stream serializationStream, object value);
		T Deserialize<T>(Stream serializationStream);
		T Deserialize<T>(byte[] buffer);
	}
}
