using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartStream
{
	public partial class ObjectWriter : IDisposable
	{
		internal struct HeaderInfo
		{
			public long PayloadBegin { get; set; }
			public long PayloadEnd { get; set; }
		}

		internal Dictionary<Type, List<HeaderInfo>> Header { get; private set; } = new Dictionary<Type, List<HeaderInfo>>();

		private bool _dirty;
		private byte[] _cachedBuffer = null;

		private RecyclableMemoryStream _blobData;
		private RecyclableMemoryStream _memoryStream;

		private BinaryWriter _writer;

		private IObjectSerializer _serializer;

		public ObjectWriter()
		{
			_memoryStream = new RecyclableMemoryStream(new RecyclableMemoryStreamManager());
			_blobData = new RecyclableMemoryStream(new RecyclableMemoryStreamManager());
			_writer = new BinaryWriter(_memoryStream);

			_serializer = new BinarySerializer();
		}

		public ObjectWriter(IObjectSerializer serializer) : this()
		{
			_serializer = serializer;
		}

		public void Write<T>(T item) where T : notnull
		{
			_dirty = true;

			if (!Header.ContainsKey(typeof(T)))
			{
				Header.Add(typeof(T), new List<HeaderInfo>());
			}

			using (var blobDataStream = new RecyclableMemoryStream(new RecyclableMemoryStreamManager()))
			{
				_serializer.Serialize(blobDataStream, item);
				var blobDataBuffer = blobDataStream.ToArray();

				var start = _blobData.Position;
				var end = start + blobDataBuffer.Length;

				Header[typeof(T)].Add(new HeaderInfo()
				{
					PayloadBegin = start,
					PayloadEnd = end
				});

				_blobData.Write(blobDataBuffer);
			}
		}

		public byte[] ToArray()
		{
			if (!_dirty)
			{
				return _cachedBuffer;
			}

			CreateStream();

			_cachedBuffer = _memoryStream.GetBuffer();
			return _cachedBuffer;
		}

		private void CreateStream()
		{
			if (!_dirty)
			{
				return;
			}

			_memoryStream.Position = 0;

			_writer.Write(0L); // Reserve memory in stream to write total size of header
			_writer.Write(Header.Count);
			foreach (var item in Header)
			{
				_writer.Write(item.Key.Name);
				_writer.Write(item.Value.Count * (sizeof(long) * 2) + sizeof(int));
				_writer.Write(item.Value.Count);

				for (int i = 0; i < item.Value.Count; i++)
				{
					var info = item.Value[i];
					_writer.Write(info.PayloadBegin);
					_writer.Write(info.PayloadEnd);
				}
			}

			var position = _memoryStream.Position;
			_memoryStream.Position = 0;
			_writer.Write(position);
			_memoryStream.Position = position;

			_writer.Write(_blobData.GetBuffer());
			_writer.Flush();

			_dirty = false;
		}

		public Stream ToStream()
		{
			CreateStream();
			return _memoryStream;
		}

		public void Dispose()
		{
			_memoryStream.Dispose();
			_blobData.Dispose();
			_writer.Dispose();
		}
	}
}
