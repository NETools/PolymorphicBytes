// - 1 -

using SmartStream;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

AllocConsole();

//var test = new Test();
//test.Data = "TestData";

//PolymorphicBufferWriter writer = new PolymorphicBufferWriter();
//writer.Write(test);
//writer.Write(test);
//writer.Write(15);
//writer.Write("This is a string!!");
//writer.Write(new Bitmap(100, 100));

//writer.Write(new int[] { 191, 215, 241 });

//var buffer = writer.ToArray();

//writer.Write("This is a string!!!!!!");
//buffer = writer.ToArray();


//File.WriteAllBytes("data", buffer);

FileStream fs = new FileStream("data", FileMode.Open);

var reader = new PolymorphicBufferReader(fs);
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

