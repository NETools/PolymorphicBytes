// - 1 -

using SmartStream;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

AllocConsole();

var test = new Test();
test.Data = "TestData";

PolymorphicBytes polymorphicStream = new PolymorphicBytes();
polymorphicStream.Write(test);
polymorphicStream.Write(test);
polymorphicStream.Write(15);
polymorphicStream.Write("This is a string!");
polymorphicStream.Write(new Bitmap(100, 100));

polymorphicStream.Write(new int[] { 191, 215, 241 });

var buffer = polymorphicStream.ToArray();

var reader = new PolymorphicBytesReader(polymorphicStream);
foreach (var item in reader.Read<int>())
{
	Console.WriteLine(item);
}

foreach (var item in reader.Read<string>())
{
	Console.WriteLine(item);
}


foreach (var item in reader.Read<Test>())
{
	Console.WriteLine(item.Data);
}

foreach (var item in reader.Read<Bitmap>())
{
	Console.WriteLine(item.Width);
}

foreach (var item in reader.Read<int[]>())
{
	for (int i = 0; i < item.Length; i++)
	{
		Console.WriteLine(item[i]);
	}
}


Console.ReadLine();


[DllImport("kernel32.dll")]
static extern int AllocConsole();

[Serializable]
public struct Test
{
	public string Data { get; set; }
}

