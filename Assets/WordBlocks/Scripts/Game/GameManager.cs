using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BizzyBeeGames.WordBlocks
{
	public class 
	GameManager : SingletonComponent<GameManager>, ISaveable
	{
		#region Inspector Variables

		[Header("Controller")]
		[SerializeField] private GameController		gameController		= null;

		[Header("Data")]
		[Tooltip("The amount of coins the player starts with.")]
		[SerializeField] private int				startingCoins		= 0;
		[Tooltip("The amount of coins the player receives for completing all levels in a pack.")]
		[SerializeField] private int				packCompleteReward	= 0;
		[Tooltip("The amount of extra word letters needed for the player to awarded the extra words coins.")]
		[SerializeField] private int				extraWordsAmount	= 0;
		[Tooltip("The amount of coins the player receives when they are awarded the extra words prize.")]
		[SerializeField] private int				extraWordsReward	= 0;
		[Tooltip("The text file containing all the extra words that can be found.")]
		[SerializeField] private TextAsset			extraWordsFile		= null;
		[Tooltip("List of all packs in the game.")]
		[SerializeField] private List<PackInfo>		packInfos			= null;
		[Tooltip("List of all hints in the game.")]
		[SerializeField] private List<HintInfo>		hintInfos			= null;

		[Header("Ads")]
		[Tooltip("Amount of time that must pass since the last interstitial ad was shown before a new interstitial ad will show.")]
		[SerializeField] private int				timeBetweenAds			= 0;
		[Tooltip("The minimum number of levels the player must start before another ad will show.")]
		[SerializeField] private int				minLevelsTillNextAd		= 0;
		[Tooltip("The minimum number of levels the player must start after they first download the app before interstitial ads start appearing.")]
		[SerializeField] private int				numLevelsTillFirstAd	= 0;
		[Tooltip("The GameObject which will show when an interstitial ad is about to show but hasn't been displayed on the screen yet.")]
		[SerializeField] private GameObject			adShowingIntercept		= null;
			
		[Header("Debug")]
		[Tooltip("If selected the GameManager will not load it's save data.")]
		[SerializeField] private bool				disableLoadSave		= false;
		[Tooltip("If selected, all levels will be un-locked and playable.")]
		[SerializeField] private bool				disableLevelLocking	= false;
		[Tooltip("If selected then hints will not cost coins to use.")]
		[SerializeField] private bool				freeHints			= false;
		[Tooltip("The level number to start on, overrides the current saved level.")]
		[SerializeField] private int				startOnLevel		= 0;
		[Tooltip("The amount of coins you will have when the game runs, overrides the saved amount of coins.")]
		[SerializeField] private int				startWithCoins		= 0;

		#endregion

		#region Member Variables

		private Dictionary<int, LevelSaveData> levelSaveDatas;

		#endregion

		#region Properties

		public string SaveId { get { return "game_manager"; } }

		public List<PackInfo>			PackInfos			{ get { return packInfos; } }
		public int						ExtraWordsAmount	{ get { return extraWordsAmount; } }
		public int						PackCompleteReward	{ get { return packCompleteReward; } }

		public List<List<LevelData>>	LevelDatas					{ get; set;}
		public HashSet<string>			ExtraWords					{ get; set;}
		public PackInfo					ActivePackInfo				{ get; set; }
		public LevelData				ActiveLevelData				{ get; set; }
		public LevelSaveData			ActiveLevelSaveData			{ get; set; }
		public int						LastCompletedLevel			{ get; set; }
		public int						NumCoins					{ get; set; }
		public int						NumExtraWordLettersFound	{ get; set; }
		public Dictionary<string, int>	HintAmounts					{ get; set; }
		public double					NextAdAt					{ get; set; }
		public int						NumLevelsTillNextAd			{ get; set; }


		// GameManager Events
		public System.Action<string>	OnHintAmountUpdated	{ get; set; }
		public System.Action			OnExtraWordFound	{ get; set; }

		// Debug Properties (Disabled in release builds)
		public bool Debug_DisableLoadSave		{ get { return Debug.isDebugBuild && disableLoadSave; } }
		public bool Debug_DisableLevelLocking	{ get { return Debug.isDebugBuild && disableLevelLocking; } }
		public bool Debug_FreeHints				{ get { return Debug.isDebugBuild && freeHints; } }

		#endregion

		#region Unity Methods

		protected override void Awake()
		{
			
			base.Awake();

			SaveManager.Instance.Register(this);

			Initialize();

			gameController.Initialize();

			CoinAnimationManager.Instance.SetCoinsText(NumCoins);
		}

		private void OnDestroy()
		{
			Save();
		}

		private void OnApplicationPause(bool pause)
		{
			if (pause)
			{
				Save();
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Starts the level.
		/// </summary>
		public void StartLevel(int packIndex, int levelIndex)
		{
			PackInfo	packInfo	= PackInfos[packIndex];
			LevelData	levelData	= packInfo.LevelDatas[levelIndex];

			StartLevel(packInfo, levelData);
		}

		/// <summary>
		/// Starts the level.
		/// </summary>
		public void StartLevel(PackInfo packInfo, LevelData levelData)
		{
			ActivePackInfo		= packInfo;
			ActiveLevelData		= levelData;
			ActiveLevelSaveData	= GetLevelSaveData(levelData);

			gameController.SetupGame(ActiveLevelData, ActiveLevelSaveData);

			ScreenManager.Instance.Show("game");

			NumLevelsTillNextAd--;
		}

		/// <summary>
		/// Starts the next level after the last completed level
		/// </summary>
		public void StartNextLevel()
		{
			PackInfo	packInfo;
			LevelData	levelData;

			if (GetPackAndLevel(LastCompletedLevel + 1, out packInfo, out levelData))
			{
				StartLevel(packInfo, levelData);
			}
		}

		/// <summary>
		/// Gets the pack info and level data using the level number
		/// </summary>
		public bool GetPackAndLevel(int levelNumber, out PackInfo packInfo, out LevelData levelData)
		{
			for (int i = 0; i < PackInfos.Count; i++)
			{
				packInfo = PackInfos[i];

				if (levelNumber <= packInfo.ToLevelNumber)
				{
					levelData = packInfo.LevelDatas[levelNumber - packInfo.FromLevelNumber];

					return true;
				}
			}

			packInfo	= null;
			levelData	= null;

			return false;
		}

		/// <summary>
		/// Uses the hint.
		/// </summary>
		public void UseHint(string hintId)
		{
			HintInfo hintInfo = GetHintInfo(hintId);

			if (hintInfo == null)
			{
				Debug.LogError("[GameManager] There is no hint with the given hint id: " + hintId);

				return;
			}

			bool useCoins;

			if (!CanUseHint(hintInfo, out useCoins))
			{
				PopupManager.Instance.Show("not_enough_coins");

				return;
			}

			bool hintUsed = false;

			switch (hintInfo.id)
			{
				case "shuffle":
					hintUsed = gameController.ShuffleBoard();
					break;
				case "letter":
					hintUsed = gameController.ShowLetterHint();
					break;
				case "tile":
					hintUsed = gameController.ShowTileHint();
					break;
				default:
					Debug.LogError("[GameManager] UseHint: Unknown hint id: " + hintId);
					break;
			}

			if (hintUsed)
			{
				if (useCoins)
				{
					AddCoins(-hintInfo.cost);
				}
				else
				{
					HintAmounts[hintInfo.id] -= 1;
				}

				if (OnHintAmountUpdated != null)
				{
					OnHintAmountUpdated(hintInfo.id);
				}

				// Play the hint used sound
				SoundManager.Instance.Play("hint_used");
			}
		}

        /// <summary>
        /// Gives hints to the player
        /// </summary>
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
				Application.Quit();
            }
        }
        public void AddHint(string hintId, int amount)
		{
			if (!HintAmounts.ContainsKey(hintId))
			{
				Debug.LogError("[GameManager] AddHint: Unknown hint id: " + hintId);
				return;
			}

			HintAmounts[hintId] += amount;

			if (OnHintAmountUpdated != null)
			{
				OnHintAmountUpdated(hintId);
			}
		}

		/// <summary>
		/// Adds the given amount of coins to the players coins
		/// </summary>
		public void AddCoins(int amount)
		{
			AddCoins(amount, true);
		}

		/// <summary>
		/// Adds the given amount of coins to the players coins
		/// </summary>
		public void AddCoins(int amount, bool setCoinText)
		{
			NumCoins += amount;

			if (setCoinText)
			{
				CoinAnimationManager.Instance.SetCoinsText(NumCoins);
			}
		}

		/// <summary>
		/// Adds tje amount of coins and animates coins
		/// </summary>
		public void AddCoinsAnimate(int amount, RectTransform fromRect, float startDelay)
		{
			// Animate the coins
			CoinAnimationManager.Instance.AnimateCoins(NumCoins, NumCoins + amount, fromRect, 100, startDelay);

			// Give the coins to the player
			NumCoins += amount;
		}

		/// <summary>
		/// Called when an extra word is found
		/// </summary>
		public void ExtraWordFound(string word)
		{
			ActiveLevelSaveData.foundExtraWords.Add(word);

			NumExtraWordLettersFound += word.Length;

			if (OnExtraWordFound != null)
			{
				OnExtraWordFound();
			}

			if (NumExtraWordLettersFound >= extraWordsAmount)
			{
				NumExtraWordLettersFound = NumExtraWordLettersFound % extraWordsAmount;

				// Animate the coins
				CoinAnimationManager.Instance.AnimateExtraWordsCoins(NumCoins, NumCoins + extraWordsReward);
					
				// Give the coins to the player
				NumCoins += extraWordsReward;
			}
		}

		/// <summary>
		/// Shows the extra words popup
		/// </summary>
		public void ShowExtraWords()
		{
			PopupManager.Instance.Show("extra_words", new object[]
			{
				extraWordsReward,
				(float)NumExtraWordLettersFound / (float)ExtraWordsAmount,
				ActiveLevelSaveData.foundExtraWords
			});
		}

		/// <summary>
		/// Gets the hint info for the given hint id
		/// </summary>
		public HintInfo GetHintInfo(string hintId)
		{
			for (int i = 0; i < hintInfos.Count; i++)
			{
				HintInfo hintInfo = hintInfos[i];

				if (hintId == hintInfo.id)
				{
					return hintInfo;
				}
			}

			return null;
		}

		/// <summary>
		/// Complete level
		/// </summary>
		public bool LevelCompleted(int levelNumber)
		{
			MobileAdsManager.Instance.ShowInterstitialAd();
			bool nextLevelCompleted = false;

			if (levelNumber == GameManager.Instance.LastCompletedLevel + 1)
			{
				GameManager.Instance.LastCompletedLevel++;

				nextLevelCompleted = true;
			}

			if (levelSaveDatas.ContainsKey(levelNumber))
			{
				LevelSaveData levelSaveData = levelSaveDatas[levelNumber];
				levelSaveData.foundLevelWords.Clear();
				levelSaveData.letterHints.Clear();
				levelSaveData.tileHints.Clear();
				levelSaveData.boardData = null;
			}

			return nextLevelCompleted;
		}

		/// <summary>
		/// Returns true if the given level number is locked
		/// </summary>
		public bool IsLevelLocked(int levelNumber)
		{
			if (Debug_DisableLevelLocking)
			{
				return false;
			}

			return levelNumber > LastCompletedLevel + 1;
		}

		/// <summary>
		/// Checks if it's time to show an ad and if so shows one
		/// </summary>
		public bool TryShowInterstitialAd(System.Action onAdFinished = null)
		{
			if (NumLevelsTillNextAd <= 0 && NextAdAt <= Utilities.SystemTimeInMilliseconds)
			{
				// Try and show an interstitial ad
				bool adShown = MobileAdsManager.Instance.ShowInterstitialAd(() =>
				{
					// The shown ad has finished
					if (onAdFinished != null)
					{
						onAdFinished();
					}
				});

				// If an ad was shown set the intercept active
				if (adShown)
				{
					// If an ad was successfully shown then reset the ad properties
					NumLevelsTillNextAd	= minLevelsTillNextAd;
					NextAdAt			= Utilities.SystemTimeInMilliseconds + timeBetweenAds * 1000;

					return true;
				}
			}

			return false;
		}

		#endregion

		#region Private Methods

		private void Initialize()
		{
			// Initialize variables
			LevelDatas		= new List<List<LevelData>>();
			levelSaveDatas	= new Dictionary<int, LevelSaveData>();
			HintAmounts		= new Dictionary<string, int>();

			// Load the save file if it exists
			if (!LoadSave())
			{
				NumCoins			= startingCoins;
				LastCompletedLevel	= 0;
				NumLevelsTillNextAd = numLevelsTillFirstAd;

				// Set the starting hint amounts
				for (int i = 0; i < hintInfos.Count; i++)
				{
					HintAmounts[hintInfos[i].id] = hintInfos[i].startAmount;
				}
			}

			// Create the games LevelDatas
			CreateLevelDatas();

			// Load all the words that are used when checking for extra words in a level
			LoadExtraWords();

			// Set the next ad timer so the user doesn't get an ad right after opening the app
			NextAdAt = Utilities.SystemTimeInMilliseconds + timeBetweenAds * 1000;

			// Set the interstitial ad events to show/hide the ad showning intercept
			MobileAdsManager.Instance.OnInterstitialAdShowing	+= () => { adShowingIntercept.SetActive(true); UIAnimation.Alpha(gameObject, 0f, 1f, 0.2f).Play(); };
			MobileAdsManager.Instance.OnInterstitialAdShown		+= () => { adShowingIntercept.SetActive(false); };

			if (Debug.isDebugBuild && startOnLevel > 0)
			{
				LastCompletedLevel = startOnLevel - 1;
			}

			if (Debug.isDebugBuild && startWithCoins > 0)
			{
				NumCoins = startWithCoins;
			}
		}

		/// <summary>
		/// Creates a LevelData for each level in each pack
		/// </summary>
		private void CreateLevelDatas()
		{
			int levelNumber = 1;

			for (int i = 0; i < PackInfos.Count; i++)
			{
				PackInfo packInfo = PackInfos[i];

				packInfo.FromLevelNumber	= levelNumber;
				packInfo.ToLevelNumber		= levelNumber + packInfo.levelFiles.Count - 1;
				packInfo.LevelDatas			= new List<LevelData>();

				LevelDatas.Add(new List<LevelData>());

				// Create a LevelData object for each level file in each pack
				for (int j = 0; j < packInfo.levelFiles.Count; j++)
				{
					LevelData levelData = new LevelData(packInfo.levelFiles[j], levelNumber + j);

					packInfo.LevelDatas.Add(levelData);

					LevelDatas[i].Add(levelData);
				}

				levelNumber += packInfo.levelFiles.Count;
			}
		}

        /// <summary>
        /// Loads the extraWordsFile
        /// </summary>
		
		private void Start()
        {
		
        }
        private void LoadExtraWords()
		{
			ExtraWords = new HashSet<string>();

			if (extraWordsFile != null)
			{
				string[] lines = extraWordsFile.text.Split('\n');

				for (int i = 0; i < lines.Length; i++)
				{
					string word = lines[i].Replace("\r", "").Trim();

					if (!ExtraWords.Contains(word))
					{
						ExtraWords.Add(word);
					}
				}
			}
		}

		/// <summary>
		/// Checks if the player can use the hint
		/// </summary>
		private bool CanUseHint(HintInfo hintInfo, out bool useCoins)
		{
			useCoins = false;

			if (Debug_FreeHints)
			{
				return true;
			}

			// Check if the player has any hints to use or if they have enough coins
			if (HintAmounts[hintInfo.id] == 0)
			{
				if (hintInfo.cost <= NumCoins)
				{
					useCoins = true;

					// The player has enough coins
					return true;
				}

				// The player has no hint amounts to use and doesn't have enough coins
				return false;
			}

			// The player has enugh hint amounts to use
			return true;
		}

		/// <summary>
		/// Gets the LevelSaveData for the given level number, if it does not already exist one will ber created
		/// </summary>
		private LevelSaveData GetLevelSaveData(LevelData levelData)
		{
			LevelSaveData levelSaveData = null;

			if (levelSaveDatas.ContainsKey(levelData.LevelNumber))
			{
				levelSaveData = levelSaveDatas[levelData.LevelNumber];

				List<string> temp = new List<string>(levelSaveData.wordsInLevel);

				foreach (string wordInLevel in levelData.Words)
				{
					if (!temp.Contains(wordInLevel))
					{
						levelSaveData = null;

						break;
					}

					temp.Remove(wordInLevel);
				}

				if (temp.Count != 0)
				{
					levelSaveData = null;
				}
			}

			if (levelSaveData == null)
			{
				levelSaveData				= new LevelSaveData();
				levelSaveData.wordsInLevel	= new List<string>(levelData.Words);
				
				levelSaveDatas[levelData.LevelNumber] = levelSaveData;
			}

			return levelSaveDatas[levelData.LevelNumber];
		}

		/// <summary>
		/// Saves to the save file
		/// </summary>
		public Dictionary<string, object> Save()
		{
			Dictionary<string, object> json = new Dictionary<string, object>();

			List<object> levelSaveDatasJson = new List<object>();

			foreach (KeyValuePair<int, LevelSaveData> pair in levelSaveDatas)
			{
				int							levelNumber			= pair.Key;
				LevelSaveData				levelSaveData		= pair.Value;
				Dictionary<string, object>	levelSaveDataJson	= new Dictionary<string, object>();

				string foundWords		= "";
				string foundExtraWords	= "";

				foreach (string value in levelSaveData.foundLevelWords)
				{
					if (!string.IsNullOrEmpty(foundWords)) foundWords += ",";
					foundWords += value;
				}

				foreach (string value in levelSaveData.foundExtraWords)
				{
					if (!string.IsNullOrEmpty(foundExtraWords)) foundExtraWords += ",";
					foundExtraWords += value;
				}

				levelSaveDataJson["level_number"]		= levelNumber;
				levelSaveDataJson["words_in_level"]		= levelSaveData.wordsInLevel;
				levelSaveDataJson["found_words"]		= foundWords;
				levelSaveDataJson["found_extra_words"]	= foundExtraWords;
				levelSaveDataJson["letter_hints"]		= SaveValues(levelSaveData.letterHints);
				levelSaveDataJson["tile_hints"]			= SaveValues(levelSaveData.tileHints);

				BoardData boardData = levelSaveData.boardData;

				if (boardData != null)
				{
					levelSaveDataJson["board_data"] = SaveBoardData(boardData);
				}

				levelSaveDatasJson.Add(levelSaveDataJson);
			}

			json["last_level_completed"]	= LastCompletedLevel;
			json["num_coins"]				= NumCoins;
			json["num_levels_till_next_ad"]	= NumLevelsTillNextAd;
			json["extra_letters"]			= NumExtraWordLettersFound;
			json["hint_amounts"]			= SaveValues(HintAmounts);
			json["level_save_data"] 		= levelSaveDatasJson;

			return json;
		}

		/// <summary>
		/// Saves the given dictionary or string/ints to a list
		/// </summary>
		private List<object> SaveValues(Dictionary<string, int> dic)
		{
			List<object> entryJsons = new List<object>();

			foreach (KeyValuePair<string, int> pair in dic)
			{
				Dictionary<string, object> entryJson = new Dictionary<string, object>();

				entryJson["key"]	= pair.Key;
				entryJson["value"]	= pair.Value;

				entryJsons.Add(entryJson);
			}

			return entryJsons;
		}

		/// <summary>
		/// Saves the board data
		/// </summary>
		private Dictionary<string, object> SaveBoardData(BoardData boardData)
		{
			Dictionary<string, object> boardDataJson = new Dictionary<string, object>();

			boardDataJson["rows"] = boardData.rows;
			boardDataJson["cols"] = boardData.cols;

			List<List<char>> grid = new List<List<char>>();

			for (int r = 0; r < boardData.rows; r++)
			{
				grid.Add(new List<char>());

				for (int c = 0; c < boardData.cols; c++)
				{
					char letter = boardData.board[r][c];

					grid[r].Add(letter == '\0' ? '-' : letter);
				}
			}

			boardDataJson["grid"] = grid;

			return boardDataJson;
		}

		/// <summary>
		/// Loads the save file
		/// </summary>
		private bool LoadSave()
		{
			JSONNode json = SaveManager.Instance.LoadSave(this);

			if (json == null)
			{
				return false;
			}

			LastCompletedLevel			= json["last_level_completed"].AsInt;
			NumCoins					= json["num_coins"].AsInt;
			NumLevelsTillNextAd			= json["num_levels_till_next_ad"].AsInt;
			NumExtraWordLettersFound	= json["extra_letters"].AsInt;
			HintAmounts					= LoadValues(json["hint_amounts"].AsArray);

			JSONArray levelSaveDatasJson = json["level_save_data"].AsArray;

			for (int i = 0; i < levelSaveDatasJson.Count; i++)
			{
				JSONObject		levelSaveDataJson	= levelSaveDatasJson[i].AsObject;
				int				levelNumber			= levelSaveDataJson["level_number"].AsInt;
				LevelSaveData	levelSaveData		= new LevelSaveData();

				levelSaveData.foundLevelWords	= new HashSet<string>(levelSaveDataJson["found_words"].Value.Split(','));
				levelSaveData.foundExtraWords	= new HashSet<string>(levelSaveDataJson["found_extra_words"].Value.Split(','));
				levelSaveData.letterHints		= LoadValues(levelSaveDataJson["letter_hints"].AsArray);
				levelSaveData.tileHints			= LoadValues(levelSaveDataJson["tile_hints"].AsArray);

				for (int j = 0; j < levelSaveDataJson["words_in_level"].AsArray.Count; j++)
				{
					levelSaveData.wordsInLevel.Add(levelSaveDataJson["words_in_level"].AsArray[j].Value);
				}

				if (levelSaveDataJson.HasKey("board_data"))
				{
					levelSaveData.boardData = LoadBoardData(levelSaveDataJson["board_data"]);
				}

				levelSaveDatas[levelNumber] = levelSaveData;
			}

			return true;
		}

		/// <summary>
		/// Loads the values
		/// </summary>
		private Dictionary<string, int> LoadValues(JSONArray entryJsons)
		{
			Dictionary<string, int> dic = new Dictionary<string, int>();

			for (int i = 0; i < entryJsons.Count; i++)
			{
				JSONNode entryJson = entryJsons[i];

				string	key		= entryJson["key"].Value;
				int		value	= entryJson["value"].AsInt;

				dic[key] = value;
			}

			return dic;
		}

		/// <summary>
		/// Loads the board data
		/// </summary>
		private BoardData LoadBoardData(JSONNode boardDataJson)
		{
			BoardData boardData = new BoardData();

			boardData.rows	= boardDataJson["rows"].AsInt;
			boardData.cols	= boardDataJson["cols"].AsInt;
			boardData.board	= new List<List<char>>();

			JSONArray gridJson = boardDataJson["grid"].AsArray;

			for (int r = 0; r < gridJson.Count; r++)
			{
				boardData.board.Add(new List<char>());

				JSONArray gridRowJson = gridJson[r].AsArray;

				for (int c = 0; c < gridRowJson.Count; c++)
				{
					string letter = gridRowJson[c].Value;

					boardData.board[r].Add(letter == "-" ? '\0' : letter[0]);
				}
			}

			return boardData;
		}

		#endregion
	}
}
