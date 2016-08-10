using System;
using System.IO.Compression;
using System.IO;

namespace Updater
{
	class Program
	{
		static void Main(string[] args)
		{
			bool Ok;

			if (args.Length != 2)
			{
				Console.WriteLine("Invalid arguments");
				return;

			}
			string ZipFileName = args[0];
			string ProgName = args[1];
			try
			{
				using (FileStream FS = new FileStream(ZipFileName, FileMode.Open))
				{
					using (ZipArchive archive = new ZipArchive(FS))
					{

						Ok=ZipArchiveExtensions.ExtractToDirectory(archive, ".", true);
					}
				}
#if MONO
				ProgName = "mono \"" + ProgName + "\"";
#endif
				System.Diagnostics.Process.Start(ProgName);
				if (Ok) File.Delete(ZipFileName);


			}
			catch (Exception Ex)
			{
				Console.WriteLine(Ex.Message);
			}
		}
	}
}
