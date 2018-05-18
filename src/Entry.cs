using System;
using System.Collections.Generic;
using System.Text;

namespace Pelasoft.JumpDir
{
	class Entry
	{
		public string Path { get; set; }

		public decimal Rank { get; set; }

		private List<string> _keys;

		public List<string> Keys
		{
			get
			{
				if (_keys == null)
				{
					_keys = new List<string>();
				}
				return _keys;
			}
			set { _keys = value; }
		}
	}
}
