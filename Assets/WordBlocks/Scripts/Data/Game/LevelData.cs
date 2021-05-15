using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BizzyBeeGames.WordBlocks
{
	public class LevelData
	{
		#region Member Variables

		private TextAsset		levelFile;
		private int				levelNumber;
		private bool			isLevelFileLoaded;
		private int				numLettersInLevel;
		private int				maxWordLength;

		// Loaded from the level file
		private string			hint;
		private List<string>	words;

		#endregion

		#region Properties

		public string		Hint				{ get { if (!isLevelFileLoaded) LoadLevelFile(); return hint; } }
		public List<string> Words				{ get { if (!isLevelFileLoaded) LoadLevelFile(); return words; } }
		public int			LevelNumber 		{ get { return levelNumber; } }
		public int			NumLettersInLevel	{ get { if (numLettersInLevel == -1) SetLettersInLevel(); return numLettersInLevel; } }
		public int			MaxWordLength		{ get { if (maxWordLength == -1) SetMaxWordLength(); return maxWordLength; } }

		#endregion

		#region Constructor

		public LevelData(TextAsset levelFile, int levelNumber)
		{
			this.levelFile			= levelFile;
			this.levelNumber		= levelNumber;
			this.isLevelFileLoaded	= false;

			numLettersInLevel	= -1;
			maxWordLength		= -1;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Checks if the given word is a word in this level
		/// </summary>
		public bool IsWordInLevel(string word)
		{
			for (int i = 0; i < Words.Count; i++)
			{
				if (word == Words[i])
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Private Methods

		private void LoadLevelFile()
		{
			isLevelFileLoaded = true;

			// Get all the lines in the levelFile
			string[] levelFileLines = levelFile.text.Split('\n');

			// Check if the file is blank
			if (levelFileLines.Length == 0)
			{
				hint	= "";
				words	= new List<string>();

				return;
			}

			// Set the hint to the first line in the file
			hint	= levelFileLines[0].Replace("\r", "").Trim();
			words	= new List<string>();

			// All other lines are words in the level
			for (int i = 1; i < levelFileLines.Length; i++)
			{
				string word = levelFileLines[i].Replace("\r", "").Trim();

				if (!string.IsNullOrEmpty(word))
				{
					if (words.Contains(word))
					{
						Debug.LogWarningFormat("[LevelData] The word \"{0}\" is duplicated in level \"{1}\", removing one of the duplicates.", word, levelFile.name);
					}
					else
					{
						words.Add(word);
					}
				}
			}

			words.Sort((string w1, string w2) => { return w1.Length - w2.Length; });
		}

		private void SetLettersInLevel()
		{
			numLettersInLevel = 0;

			for (int i = 0; i < Words.Count; i++)
			{
				numLettersInLevel += Words[i].Length;
			}
		}

		private void SetMaxWordLength()
		{
			maxWordLength = int.MinValue;

			for (int i = 0; i < Words.Count; i++)
			{
				maxWordLength = Mathf.Max(maxWordLength, Words[i].Length);
			}
		}

		#endregion
	}
}