using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BizzyBeeGames.WordBlocks
{
	public class LevelSaveData
	{
		#region Member Variables

		public List<string>				wordsInLevel	= new List<string>();
		public HashSet<string>			foundLevelWords	= new HashSet<string>();
		public HashSet<string>			foundExtraWords	= new HashSet<string>();
		public Dictionary<string, int>	letterHints		= new Dictionary<string, int>();
		public Dictionary<string, int>	tileHints		= new Dictionary<string, int>();
		public BoardData				boardData		= null;

		#endregion
	}
}
