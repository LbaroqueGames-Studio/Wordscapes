using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BizzyBeeGames.WordBlocks
{
	public class ThemeListItem : ClickableListItem
	{
		#region Inspector Variables

		[SerializeField] private Image		themeImage			= null;
		[SerializeField] private Text		themeNameText		= null;
		[SerializeField] private Image		borderImage			= null;
		[SerializeField] private Color		borderNormalColor	= Color.white;
		[SerializeField] private Color		borderSelectedColor	= Color.white;
		[SerializeField] private GameObject	lockedContainer		= null;
		[SerializeField] private Text		coinsAmountText		= null;

		#endregion

		#region Public Methods

		public void Setup(ThemeManager.Theme theme)
		{
			themeImage.sprite	= theme.listItemImage;
			themeNameText.text	= theme.name;

			lockedContainer.SetActive(ThemeManager.Instance.IsThemeLocked(theme));
			coinsAmountText.text = theme.coinsToUnlock.ToString();
		}

		public void SetSelected(bool isSelected)
		{
			borderImage.color = isSelected ? borderSelectedColor : borderNormalColor;
		}

		#endregion
	}
}
