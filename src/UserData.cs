using System;
using System.Collections.Generic;
using System.Text;

namespace Pelasoft.JumpDir
{
	class UserData
	{
		public List<Entry> Entries { get; set; }

		public string LastPath { get; set; }
		public string LastSearch { get; set; }
		public List<string> LastCandidates { get; set; }
		public DateTime LastAccess { get; set; }

		public UserData()
		{
			Entries = new List<Entry>();
		}

	}
}
