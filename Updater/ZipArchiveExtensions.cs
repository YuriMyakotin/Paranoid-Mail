using System.IO.Compression;
using System.IO;
using System;
using System.Threading;

namespace Updater
{
	public static class ZipArchiveExtensions
	{
		public static bool ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
		{
			if (!overwrite)
			{
				archive.ExtractToDirectory(destinationDirectoryName);
				return true;
			}
			foreach (ZipArchiveEntry file in archive.Entries)
			{
				string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
				string directory = Path.GetDirectoryName(completeFileName);

				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				if ((file.Name != "") && (file.Name != "Updater.exe"))
				{
					try
					{
						file.ExtractToFile(completeFileName, true);
						continue;
					}
					catch
					{
						Thread.Sleep(15000);
					}
					try
					{
						file.ExtractToFile(completeFileName, true);
					}
					catch (Exception Ex)
					{
						Console.WriteLine(Ex.Message);
						return false;
					}
				}
			}
			return true;
		}
	}
}
