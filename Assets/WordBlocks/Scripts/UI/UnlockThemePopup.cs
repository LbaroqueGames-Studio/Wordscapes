using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BizzyBeeGames.WordBlocks
{
	public class UnlockThemePopup : Popup
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private Image	listItemImage	= null;
		[SerializeField] private Text	costText		= null;

		#endregion

		#region Public Methods

		public override void OnShowing(object[] inData)
		{
			listItemImage.sprite	= inData[0] as Sprite;
			costText.text			= ((int)inData[1]).ToString();
		}

		#endregion
	}
}
