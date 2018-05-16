using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pelasoft.JumpDir
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Error.WriteLine("Args:");
			args.ToList().ForEach(x => Console.Error.WriteLine($"   {x}"));
			Console.Error.WriteLine();
			var dirs = Directory.GetDirectories(Path.GetFullPath(".")).ToList();
			Console.Error.WriteLine("Dirs:");
//			dirs.ForEach(x => Console.Error.WriteLine($"   {x}"));

			Func<string, bool> predicate = x => true; // default predicate, take everything

			if (args.Length > 0)
			{
				dirs = dirs.Where(x => x.ToLower().Contains(args[0].ToLower())).ToList();
			}
			if (dirs.Count() == 1)
			{
				Console.WriteLine(dirs.First());
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
