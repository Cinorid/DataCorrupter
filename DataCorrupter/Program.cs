using System.CommandLine;

namespace DataCorrupter;

class Program
{
	static async Task<int> Main(string[] args)
	{
		var inputFile = new Option<FileInfo?>(
			new[] { "--file", "-f" },
			"Input file name or path")
		{
			IsRequired = true,
			ArgumentHelpName = "inputFile"
		};

		var outputFile = new Option<FileInfo?>(
			new[] { "--out", "-o" },
			"Output file name or path")
		{
			ArgumentHelpName = "outputFile"
		};

		var ratio = new Option<int>(
			new[] { "--ratio", "-r" },
			"insert 1 random bytes per N bytes")
		{
			ArgumentHelpName = "ratioNumber",
		};
		ratio.SetDefaultValue(1000);

		var inplace = new Option<bool>(
			new[] { "--inplace", "-p" },
			"[WARNING] makes all changes into input");
		inplace.SetDefaultValue(false);

		var rootCommand = new RootCommand()
		{
			Name = "corrupter",
			Description = "Corrupt any file with random data.",
		};
		rootCommand.AddOption(inputFile);
		rootCommand.AddOption(outputFile);
		rootCommand.AddOption(ratio);
		rootCommand.AddOption(inplace);
		
		rootCommand.SetHandler(
			DoOperation,
			inputFile,
			outputFile,
			ratio,
			inplace);

		return await rootCommand.InvokeAsync(args);
	}

	static void DoOperation(FileInfo? inputFile, FileInfo? outputFile, int ratio, bool inplace)
	{
		FileStream inFile;
		FileStream outFile;

		

		if (inputFile?.Exists == false)
		{
			Console.WriteLine($"file \"{inputFile.FullName}\" not found.");
			return;
		}
		
		if (inplace)
		{
			if (inputFile is null)
				return;
			
			inFile = inputFile.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			outFile = inFile;
		}
		else
		{
			if (outputFile is null)
			{
				Console.WriteLine("when --inplace is false, --out should be specified.");
				return;
			}
			
			if (inputFile is null)
				return;
			
			inFile = inputFile.OpenRead();
			outFile = outputFile.OpenWrite();
		}

		var buffer = new byte[4096];

		if (!inplace)
		{
			var readCount = 0;
			var totalCounter = 0;
			do
			{
				readCount = inFile.Read(buffer, 0, buffer.Length);
				outFile.Write(buffer, 0, readCount);
				totalCounter += readCount;

				if (totalCounter % 10_000 == 0)
					Console.WriteLine($"Copy Base Data {Math.Round((decimal)totalCounter / inFile.Length * 100, 2)} %");
			} while (readCount > 0);
			
			Console.WriteLine("Copy Base Data 100.00 %");
		}

		var numOfRandomWrite = inFile.Length / ratio;

		Random random = new Random();
		outFile.Seek(0, SeekOrigin.Begin);
		for (long i = 0; i < numOfRandomWrite; i++)
		{
			var pointer = i * ratio + random.Next(ratio);
			if (pointer >= inFile.Length)
				continue;

			outFile.Seek(pointer, SeekOrigin.Begin);

			outFile.WriteByte(Convert.ToByte(random.Next(0, byte.MaxValue)));

			if (i % 10_000 == 0)
				Console.WriteLine($"Corrupting Data {Math.Round((decimal)pointer / inFile.Length * 100, 2)} %");
		}
		Console.WriteLine("Corrupting Data 100.00 %");

		inFile.Close();

		outFile.Flush();
		outFile.Close();
	}
}