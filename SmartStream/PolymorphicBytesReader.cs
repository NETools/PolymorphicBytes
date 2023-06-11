using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartStream
{
	public class PolymorphicBytesReader
	{
		public PolymorphicBytes BaseBuffer { get; private set; }

		private BinaryReader _reader;
		private IPolymorphicSerializer _serializer;

		public PolymorphicBytesReader(PolymorphicBytes bytes)
		{
			BaseBuffer = bytes;

			_reader = new BinaryReader(BaseBuffer.ToStream());
			_serializer = new BinarySerializer();
		}

		public PolymorphicBytesReader(PolymorphicBytes bytes, IPolymorphicSerializer serializer) : this(bytes)
		{
			_serializer = serializer;
		}

		public IEnumerable<T> Read<T>()
		{
			_reader.BaseStream.Position = 0;

			string typeName = typeof(T).Name;

			var headerSize = _reader.ReadInt64();
			var headerCount = _reader.ReadInt32();
			for (int i = 0; i < headerCount; i++)
			{
				var name = _reader.ReadString();
				var totalSize = _reader.ReadInt32(); 
				if (name == typeName)
				{
					int headerInfoCount = _reader.ReadInt32();

					for (int j = 0; j < headerInfoCount; j++)
					{
						long payloadBegin = _reader.ReadInt64();
						long payloadEnd = _reader.ReadInt64();

						var buffer = new byte[payloadEnd - payloadBegin];
						long position = _reader.BaseStream.Position;

						_reader.BaseStream.Seek(payloadBegin + headerSize, SeekOrigin.Begin);
						_reader.BaseStream.ReadExactly(buffer);
						_reader.BaseStream.Seek(position, SeekOrigin.Begin);

						using (var blobDataStream = new MemoryStream(buffer))
						{
							yield return _serializer.Deserialize<T>(blobDataStream);
						}
					}
				}
				else
				{
					_reader.BaseStream.Seek(totalSize, SeekOrigin.Current);
				}
			}
		}
	}
}
