using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BizzyBeeGames.WordBlocks
{
	[System.Serializable]
	public class PackInfo
	{
		#region Member Variables

		public string			displayName;
		public List<TextAsset>	levelFiles;

		#endregion

		#region Properties

		public int				FromLevelNumber	{ get; set; }
		public int				ToLevelNumber	{ get; set; }
		public int				NumLevelsInPack	{ get { return ToLevelNumber - FromLevelNumber + 1;} }
		public List<LevelData>	LevelDatas		{ get; set; }

		#endregion
	}
}
