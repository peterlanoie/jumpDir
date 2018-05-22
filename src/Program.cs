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

		private Regex _backRefExp = new Regex(@"^(?<backref>[\.\\][\.\\]*)(?<dir>[^ ]+)?$");

		private bool _verbose = false;
		private string _userDataFilePath;
		private UserData _userData;
		private int _repeatTimeout = 10;
		private int _flyoverTimeout = 10; // max seconds before a subsequent jump purges a new entry

		static void Main(string[] args)
		{
			new Program().Run(args);
		}

		private void Run(string[] args)
		{
			var doCD = true;
			var hookCommand = "CD";

			var commands = args.Where(x => x.StartsWith('-')).Select(x => x.ToLower().TrimStart('-'));

			if (commands.Any(x => x == "v" || x == "verbose"))
			{
				_verbose = true;
				Log("running in verbose mode\n");
			}

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

			Verbose(() => $"captured command(s): { string.Join(' ', commands) }\n");

			args = args.Where(x => !x.StartsWith('-')).ToArray();

			if (commands.Count() > 0)
			{
				foreach (var command in commands)
				{
					switch (command)
					{
						case "list":
						case "stats":
						case "s":
							ShowStats();
							doCD = false;
							break;

						case "delete":
						case "d":
							if (args.Length > 0)
							{
								DeleteKeys(args);
							}
							else
							{
								Log("!! no search term(s) specified to delete");
							}
							doCD = false;
							break;

						case "pushd":
							hookCommand = command;
							break;

						case "popd":
							Console.WriteLine("POPD");
							doCD = false;
							break;

						case "clear":
						case "c":
							ClearEntries();
							doCD = false;
							break;
					}
				}
			}
			if (doCD)
			{
				var flyOvers = _userData.Entries.Where(x => (DateTime.Now - x.LastUsed).TotalSeconds < _flyoverTimeout).ToList();
				if (flyOvers.Count > 0)
				{
					foreach (var flyOver in flyOvers)
					{
						if (flyOver.Rank > 1)
						{
							flyOver.Rank--;
							Verbose(() => $"deranking {flyOver.Path}");
						}
						else
						{
							Verbose(() => $"removing {flyOver.Path} flyover entry");
							_userData.Entries.Remove(flyOver);
						}
					}
					Verbose(() => "\n");
				}
				var dir = FindDirectory(args);
				if (!string.IsNullOrWhiteSpace(dir))
				{
					var command = hookCommand;
					if(dir.Contains(' ')){
						dir = $"\"{dir}\"";
					}
					command += $" {dir}";
					Verbose(() => $"\ncommand to be issued: {command}");
					Console.WriteLine(command);
				}
			}

			_userData.LastAccess = DateTime.Now;
			File.WriteAllText(_userDataFilePath, JsonConvert.SerializeObject(_userData, Formatting.Indented));
		}

		private void DeleteKeys(string[] args)
		{
			var argList = args.ToList();
			_userData
				.Entries.Where(x => x.Keys.Any(y => args.Any(z => y.Equals(z, StringComparison.InvariantCultureIgnoreCase))))
				.ToList()
				.ForEach(x => argList.ForEach(y => {
						Verbose(() => $"removing keys in matched path '{x.Path}'");
					for(var i = x.Keys.Count-1; i >= 0; i--){
						Verbose(() => $"comparing argument '{y}' with '{x.Keys[i]}' for possible removal");
						if(x.Keys[i].Equals(y, StringComparison.InvariantCultureIgnoreCase)){
							Log($" removed key '{x.Keys[i]}' for path '{x.Path}'");
							x.Keys.RemoveAt(i);
						}
					}
				}));
		}

		private void ShowStats()
		{
			Log(" jumpDir usage statistics\n");

			if (_userData.Entries.Count() > 0)
			{
				Log(" Rank    Path [key(s)]");
				var maxWidth = _userData.Entries.Max(x => x.Path.Length);
				Log(" ==========================================");
				foreach (var entry in _userData.Entries.OrderByDescending(x => x.Rank))
				{
					Log($"  {entry.Rank.ToString().PadLeft(6)}  {entry.Path}  [{ string.Join('|', entry.Keys)}]");
				}
				Log("\n use '-[c]lear' to reset all usage");
				Log(" use 'key [key...n] -[d]elete' to delete individual key(s)");
			}
			else
			{
				Log(" no jumpDir items yet");
			}
			Log();
		}

		private string FindDirectory(string[] args)
		{
			string searchDir = args.Length > 0 ? args[0] : null;
			var saveKey = args.Length > 1 ? args[1] : null;
			var lastSearch = _userData.LastSearch;
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

					if (searchDir == ".")
					{
						// update entries with current path using the provided search or the full current directory name
						saveKey = saveKey ?? Path.GetFileName(targetPath);
						UpdateDirectoryUse(targetPath, saveKey, false, true);
						Log($" jumpDir entry updated for {saveKey} => {targetPath}");
						Log($" remove it with 'jd {saveKey} -d'");
					}

					if (!dirGroup.Success)
					{
						return searchDir;
					}
					searchDir = dirGroup.Value;
				}
			}
			var dirs = Directory.GetDirectories(targetPath);
			_userData.LastSearch = searchDir;
			if (searchDir != null)
			{
				List<string> candidates = null;
				var targetIndex = 0;

				var isRepeat = ((DateTime.Now - _userData.LastAccess).TotalSeconds < _repeatTimeout) // in the list command time window
					&& (int.TryParse(searchDir, out targetIndex) || lastSearch == searchDir); // same search or line number

				if (isRepeat)
				{ // in the list command time window
					candidates = _userData.LastCandidates;
					if (targetIndex == 0)
					{ // repeated search
						targetIndex = _userData.LastCandidates.IndexOf(_userData.LastPath) + 1;
					}
					else
					{
						_userData.LastSearch = searchDir = lastSearch;
						targetIndex--; // correct for user indexing
						saveKey = Path.GetFileName(candidates[targetIndex]);
					}
					if (targetIndex >= _userData.LastCandidates.Count || targetIndex < 0)
					{
						targetIndex = 0;
					}
				}
				else
				{ // fresh call
					targetIndex = 0;
					candidates = new List<string>();
					candidates.AddRange(_userData.Entries.OrderByDescending(x => x.Rank)
						.Where(x => x.Keys.Any(y => searchDir.Equals(y, StringComparison.InvariantCultureIgnoreCase)))
						.Select(x => x.Path)
					);
					candidates.AddRange(dirs.Where(x => Path.GetFileName(x).StartsWith(searchDir, StringComparison.InvariantCultureIgnoreCase)));
					candidates.AddRange(dirs.Where(x => Path.GetFileName(x).ToLower().Contains(searchDir.ToLower())));
					_userData.LastCandidates = candidates = candidates.Distinct().ToList();
				}

				if (candidates.Count > 0)
				{
					var chosenOne = candidates[targetIndex];
					if (candidates.Count > 1)
					{
						for (int i = 0, listCount = candidates.Count; i < listCount; i++)
						{
							var candidate = candidates[i];
							Log($"{(candidate == chosenOne ? "==>" : "   ")} {(i + 1).ToString().PadLeft(2)}  {candidate}{(candidate == chosenOne ? "  <==" : "")}");
						}
						Log($"\n within {_repeatTimeout} seconds:");
						Log($"   repeat `jd {searchDir}` to go to next in list");
						Log($"   `jd {{number}}` to go to numbered entry");
					}
					return UpdateDirectoryUse(chosenOne, saveKey ?? searchDir, true, saveKey == null);
				}
				else
				{
					Log($"\n no directory matches found for '{searchDir}'\n");
				}

			}

			if (dirs.Count() > 0)
			{
				Log(" possible jump target dirs:");
				dirs.ToList().ForEach(x => Log($"   {x}"));
			}
			else
			{
				Log(" sorry, possible no target directories found");
			}
			return null;
		}

		private string UpdateDirectoryUse(string path, string search, bool saveLastPath, bool allowFlyover)
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
			entry.LastUsed = DateTime.Now.AddSeconds(allowFlyover ? 0 : -_flyoverTimeout); // to avoid flyover flushing, force time back
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
			if (saveLastPath)
			{
				_userData.LastPath = path;
			}
			return path;
		}

		private void ClearEntries()
		{
			var count = _userData.Entries.Count;
			if (count > 0)
			{
				Log($" Are you sure you want to clear all {count} item(s) (press Y to confirm)? ", false);
				var confirm = Console.ReadKey(true);
				Log();
				if(confirm.Key == ConsoleKey.Y){
					_userData.LastCandidates.Clear();
					_userData.LastPath = null;
					_userData.LastSearch = null;
					_userData.Entries.Clear();
					Log($" {count} jumpDir item(s) deleted");
				} else {
					Log(" clear cancelled");
				}
			}
			else
			{
				Log(" no jumpDir items to delete");
			}
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


		private void Log(string message = null, bool newLine = true)
		{
			if(newLine)
			{
				Console.Error.WriteLine(message);
			} else {
				Console.Error.Write(message);
			}
		}

	}
}
