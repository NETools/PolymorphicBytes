using System.Runtime.Serialization.Formatters.Binary;

namespace SmartStream
{
	public class BinarySerializer : IObjectSerializer
	{
		private BinaryFormatter _binaryFormatter = new BinaryFormatter();

		public T Deserialize<T>(Stream serializationStream)
		{
#pragma warning disable SYSLIB0011 // Typ oder Element ist veraltet
			return (T)_binaryFormatter.Deserialize(serializationStream);
#pragma warning restore SYSLIB0011 // Typ oder Element ist veraltet
		}

		public T Deserialize<T>(byte[] buffer)
		{
#pragma warning disable SYSLIB0011 // Typ oder Element ist veraltet
			return (T)_binaryFormatter.Deserialize(new MemoryStream(buffer));
#pragma warning restore SYSLIB0011 // Typ oder Element ist veraltet
		}

		public void Serialize(Stream serializationStream, object value)
		{
#pragma warning disable SYSLIB0011 // Typ oder Element ist veraltet
			_binaryFormatter.Serialize(serializationStream, value);
#pragma warning restore SYSLIB0011 // Typ oder Element ist veraltet
		}
	}
}