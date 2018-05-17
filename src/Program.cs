using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pelasoft.JumpDir
{
	class Program
	{
		private const string _userDataDir = ".jumpDir";
		private const string _userDataFile = "userdata.json";

		static void Main(string[] args)
		{
			Console.Error.WriteLine("Args:");
			args.ToList().ForEach(x => Console.Error.WriteLine($"   {x}"));
			Console.Error.WriteLine();
			var dirs = Directory.GetDirectories(Path.GetFullPath(".")).ToList();
			Console.Error.WriteLine("Dirs:");

			string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
				   Environment.OSVersion.Platform == PlatformID.MacOSX)
					? Environment.GetEnvironmentVariable("HOME")
					: Environment.ExpandEnvironmentVariables("%USERPROFILE%");

			var userDataDirPath = Path.Combine(homePath, _userDataDir);
			var userDataFilePath = Path.Combine(userDataDirPath, _userDataFile);

			Console.Error.WriteLine($"User data file: {userDataFilePath}");

			UserData userData;

			if (File.Exists(userDataFilePath))
			{
				userData = JsonConvert.DeserializeObject<UserData>(File.ReadAllText(userDataFilePath));
			}
			else
			{
				if (!Directory.Exists(userDataDirPath))
				{
					Directory.CreateDirectory(userDataDirPath);
				}
				userData = new UserData();
			}

			Func<string, bool> predicate = x => true; // default predicate, take everything

			if (args.Length > 0)
			{
				dirs = dirs.Where(x => x.ToLower().Contains(args[0].ToLower())).ToList();
			}
			if (dirs.Count() == 1)
			{
				var targetPath = dirs.First();
				Console.WriteLine(targetPath);
				var entry = userData.Entries.FirstOrDefault(x => x.Path == targetPath);
				if (entry == null)
				{
					userData.Entries.Add(entry = new Entry { Path = targetPath, LaunchCount = 1 });
				}
				else
				{
					entry.LaunchCount++;
				}
				userData.LastPath = targetPath;
				File.WriteAllText(userDataFilePath, JsonConvert.SerializeObject(userData, Formatting.Indented));
				return;
			}
			if (dirs.Count() > 1)
			{
				Console.Error.WriteLine("Possible targets:");
				dirs.ForEach(x => Console.Error.WriteLine($"   {x}"));
			}
			Console.WriteLine("[no target]");

		}
	}
}
