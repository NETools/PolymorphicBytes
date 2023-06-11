﻿using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartStream
{
	public class PolymorphicBufferReader
	{
		private BinaryReader _reader;
		private IPolymorphicSerializer _serializer;

		public PolymorphicBufferReader(byte[] buffer)
		{
			_reader = new BinaryReader(new MemoryStream(buffer));
			_serializer = new BinarySerializer();
		}

		public PolymorphicBufferReader(byte[] buffer, IPolymorphicSerializer serializer) : this(buffer)
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

						_reader.BaseStream.Position = payloadBegin + headerSize;
						_reader.BaseStream.ReadExactly(buffer);
						_reader.BaseStream.Position = position;

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