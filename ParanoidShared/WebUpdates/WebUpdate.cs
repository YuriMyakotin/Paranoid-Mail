using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Paranoid
{
	public static class WebUpdate
	{

		public static UpdateInfoDataModel CheckForUpdate(long ProductID, long BuildNumber, UpdateType CheckType)
		{
			if (CheckType == UpdateType.Disabled) return null;

			CheckUpdateDataModel RequestData = new CheckUpdateDataModel
			{
				ProductID = ProductID,
				CurrentBuild = BuildNumber,
				CheckType = (int)CheckType,
				SaltBytes=new byte[32]
			};
			ParanoidRNG.GetBytes(RequestData.SaltBytes,0,32);

			byte[] RequestBytes = Utils.ObjectToBytes(RequestData);

			byte[] ReplyBytes=WebInfrastructure.GetDataFromWeb("Updates", RequestBytes);
			if (ReplyBytes == null) return null;


			if (ReplyBytes.Length <= sizeof(long)) return null;
			try
			{
				byte[] UnzippedBytes = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(ReplyBytes);
				return Utils.BytesToObject<UpdateInfoDataModel>(UnzippedBytes);
			}
			catch
			{
				return null;
			}


		}

		private static byte[] GetUpdateFile(long ProductID, long BuildNumber, UpdateType CheckType)
		{
			if (CheckType == UpdateType.Disabled) return null;

			CheckUpdateDataModel RequestData = new CheckUpdateDataModel
			{
				ProductID = ProductID,
				CurrentBuild = BuildNumber,
				CheckType = (int)CheckType,
				SaltBytes = new byte[32]
			};
			ParanoidRNG.GetBytes(RequestData.SaltBytes, 0, 32);

			byte[] RequestBytes = Utils.ObjectToBytes(RequestData);

			byte[] ReplyBytes = WebInfrastructure.GetDataFromWeb(@"Updates/GetUpdatedFile", RequestBytes);
			if (ReplyBytes == null) return null;


			return ReplyBytes.Length == sizeof(long) ? null : ReplyBytes;
		}


		public static bool UpdateApp(long ProductID, long BuildNumber, UpdateType CheckType)
		{
			string ProgramExePath = Assembly.GetEntryAssembly().Location;
			string ProgramFolder = Path.GetDirectoryName(ProgramExePath);
			string ProgramName = Path.GetFileNameWithoutExtension(ProgramExePath);
			if (ProgramFolder == null) return false;
			string ZipFileName = Path.Combine(ProgramFolder, ProgramName + ".zip");
			string UpdaterName = Path.Combine(ProgramFolder, "Updater.exe");


			byte[] ZipFileBytes = GetUpdateFile(ProductID, BuildNumber, CheckType);
			if (ZipFileBytes == null) return false;

			File.WriteAllBytes(ZipFileName, ZipFileBytes);

			using (MemoryStream MS = new MemoryStream(ZipFileBytes))
			{
				using (ZipArchive archive = new ZipArchive(MS,ZipArchiveMode.Read))
				{
					ZipArchiveEntry entry=archive.Entries.SingleOrDefault(p => p.Name == "Updater.exe");
					if (entry == null) return false;
					try
					{
						entry.ExtractToFile(UpdaterName,true);
						#if MONO
						System.Diagnostics.Process.Start("mono", UpdaterName+" \""+ZipFileName + "\" \"" + ProgramExePath + "\"");
						#else
						System.Diagnostics.Process.Start(UpdaterName, "\""+ZipFileName + "\" \"" + ProgramExePath + "\"");
						#endif 
						return true;
					}
					catch
					{
						return false;
					}
				}
			}


		}
	}
}
