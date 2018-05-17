using System;
using System.Collections.Generic;
using System.Text;

namespace Pelasoft.JumpDir
{
	class Entry
	{
		public string Path { get; set; }

		public decimal Rank { get; set; }

		public List<string> Keys { get; set; }
	}
}
