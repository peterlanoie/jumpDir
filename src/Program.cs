using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pelasoft.JumpDir
{
	class Program
	{
		private const string _userDataDir = ".jumpDir";
		private const string _userDataFile = "userdata.json";
		private const string _noTargetResponse = "[no target]";

		private Regex _backRefExp = new Regex(@"(?<backref>[\.\\][\.\\]*)(?<dir>[^ ]+)?");

		private bool _verbose = false;
		private string _userDataFilePath;
		private UserData _userData;

		static void Main(string[] args)
		{
			new Program().Run(args);
		}

		private void Run(string[] args)
		{
			var doCD = true;

			Verbose(() => $"raw args: { string.Join(' ', args)}\n");
			//			args.ToList().ForEach(x => Verbose($"   {x}"));

			string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
				   Environment.OSVersion.Platform == PlatformID.MacOSX)
					? Environment.GetEnvironmentVariable("HOME")
					: Environment.ExpandEnvironmentVariables("%USERPROFILE%");

			var userDataDirPath = Path.Combine(homePath, _userDataDir);
			_userDataFilePath = Path.Combine(userDataDirPath, _userDataFile);

			Verbose(() => $"user data file: {_userDataFilePath}\n");

			if (File.Exists(_userDataFilePath))
			{
				_userData = JsonConvert.DeserializeObject<UserData>(File.ReadAllText(_userDataFilePath));
			}
			else
			{
				if (!Directory.Exists(userDataDirPath))
				{
					Directory.CreateDirectory(userDataDirPath);
				}
				_userData = new UserData();
			}

			var commands = args.Where(x => x.StartsWith('-')).Select(x => x.ToLower().TrimStart('-'));
			Verbose(() => $"captured command(s): { string.Join(' ', commands) }");

			if (commands.Count() > 0)
			{
				foreach (var command in commands)
				{
					switch (command)
					{
						case "clear":
						case "c":
							ClearEntries();
							doCD = false;
							break;

						case "stats":
						case "s":
							ShowStats();
							doCD = false;
							break;

						case "verbose":
						case "v":
							_verbose = true;
							break;
					}
				}

				Console.WriteLine(_noTargetResponse);
			}
			if (doCD)
			{
				args = args.Where(x => !x.StartsWith('-')).ToArray();
				Console.WriteLine(FindDirectory(args.Length > 0 ? args[0] : null));
			}
		}

		private void ShowStats()
		{
			Log("  jumpDir usage statistics\n");

			if (_userData.Entries.Count() > 0)
			{
				Log("  Rank    Path");
				var maxWidth = _userData.Entries.Max(x => x.Path.Length);
				Log("  ==========================================");
				foreach (var entry in _userData.Entries.OrderByDescending(x => x.Rank))
				{
					Log($"  {entry.Rank.ToString().PadLeft(6)}  {entry.Path}");
				}
				Log("\n  Use the '-[c]lear' command to reset all usage.");
			}
			else
			{
				Log("No entries yet.");
			}
			Log();
		}

		private string FindDirectory(string searchDir)
		{
			var targetPath = Path.GetFullPath(".");

			if (searchDir != null)
			{
				Verbose(() => $"finding possible directories for: {searchDir}");
				var backRefMatch = _backRefExp.Match(searchDir);

				if (backRefMatch.Success)
				{
					var backRef = backRefMatch.Groups["backref"].Value;
					Verbose(() => $"found backreference: {backRef}");
					targetPath = Path.GetFullPath(backRef);
					Verbose(() => $"updated target search path to: {targetPath}\n");
					var dirGroup = backRefMatch.Groups["dir"];
					if (!dirGroup.Success)
					{
						return searchDir;
					}
					searchDir = dirGroup.Value;
				}
			}
			var dirs = Directory.GetDirectories(targetPath);
			//				.Select(x => x.Replace(targetPath, "", StringComparison.InvariantCultureIgnoreCase));
			//				.Select(x => Path.GetFileName(x));
			if (searchDir != null)
			{
				var searches = new Func<string, string, bool>[]
				{
					(dir, search) => Path.GetFileName(dir).StartsWith(search, StringComparison.InvariantCultureIgnoreCase), // 1. dirs that START with the arg
					(dir, search) => Path.GetFileName(dir).ToLower().Contains(search.ToLower()),   // 2. dirs that CONTAIN the arg
				};

				foreach (var search in searches)
				{
					var results = dirs.Where(dir => search(dir, searchDir)).ToArray();
					if (results.Count() == 1)
					{
						// if theres 1, go to it
						return UpdateDirectoryUse(results.First(), searchDir);
					}
					else if (results.Count() > 1)
					{
						// if there's more than 1, update the main list
						dirs = results;
					}
				}
			}

			if (dirs.Count() > 1)
			{
				Log("Possible targets:");
				dirs.ToList().ForEach(x => Log($"   {x}"));
			}
			else
			{
				Log("Sorry, no target directories found");
			}
			return _noTargetResponse;
		}

		private string UpdateDirectoryUse(string path, string search)
		{
			var entry = _userData.Entries.FirstOrDefault(x => x.Path == path);
			if (entry == null)
			{
				_userData.Entries.Add(entry = new Entry { Path = path, Rank = 1 });
			}
			else
			{
				entry.Rank++;
			}
			if (search.Length >= 2)
			{
				if (entry.Keys == null)
				{
					entry.Keys = new List<string>();
				}
				var key = entry.Keys.SingleOrDefault(x => x.Equals(search, StringComparison.InvariantCultureIgnoreCase));
				if (key == null)
				{
					entry.Keys.Add(search);
				}
			}

			_userData.LastPath = path;
			SaveData();
			return path;
		}

		private void ClearEntries()
		{
			_userData.Entries.Clear();
			SaveData();
		}

		private void SaveData()
		{
			File.WriteAllText(_userDataFilePath, JsonConvert.SerializeObject(_userData, Formatting.Indented));
		}

		//private void Verbose(string message = null)
		//{
		//	Verbose(() => message);
		//}

		private void Verbose(Func<string> messageFunc)
		{
			if (_verbose)
			{
				Console.Error.WriteLine(messageFunc());
			}
		}


		private void Log(string message = null)
		{
			Console.Error.WriteLine(message);
		}

	}
}
