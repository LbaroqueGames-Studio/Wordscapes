using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BizzyBeeGames.WordBlocks
{
	public class GridLetterTile : LetterTile, IThemeBehaviour
	{
		#region Inspector Variables

		[Space]
		[Header("Grid Tile")]
		[SerializeField] private string 	normalBkgColorThemeId		= "";
		[SerializeField] private string 	normalLetterColorThemeId	= "";
		[SerializeField] private string 	selectedBkgColorThemeId		= "";
		[SerializeField] private string 	selectedLetterColorThemeId	= "";
		[SerializeField] private string 	hintBkgColorThemeId			= "";
		[SerializeField] private string 	hintLetterColorThemeId		= "";

		[Space]
		[Header("Anim Settings - Shake")]
		[SerializeField] private int	shakeAmount			= 0; 
		[SerializeField] private float	shakeForce			= 0; 
		[SerializeField] private float	shakeAnimDuration	= 0; 

		[Space]
		[Header("Anim Settings - Twist")]
		[SerializeField] private int	twistAmount			= 0; 
		[SerializeField] private float	twistForce			= 0; 
		[SerializeField] private float	twistAnimDuration	= 0;

		[Space]
		[Header("Anim Settings - Pulse")]
		[SerializeField] private int	pulseAmount			= 0; 
		[SerializeField] private float	pulseForce			= 0; 
		[SerializeField] private float	pulseAnimDuration	= 0;

		[Space]
		[Header("Anim Settings - Durations")]
		[SerializeField] private float	fadeAnimDuration	= 0; 
		[SerializeField] private float	moveAnimDuration	= 0; 
		[SerializeField] private float	hintAnimDuration	= 0; 

		#endregion

		#region Member Variables

		private bool	initialized;
		private Vector2	scale;
		private Vector2	position;


		private bool isHintDisplayed;

		private Color normalBkgColor;
		private Color normalLetterColor;
		private Color selectedBkgColor;
		private Color selectedLetterColor;
		private Color hintBkgColor;
		private Color hintLetterColor;

		#endregion

		#region Properties

		public bool IsHintDisplayed { get { return isHintDisplayed; } }

		#endregion

		#region Public Methods

		public void NotifyThemeChanged()
		{
			SetColors();
		}

		public override void Setup(char letter)
		{
			if (!initialized)
			{
				Initialize();
			}

			base.Setup(letter);

			this.scale		= transform.localScale;
			this.position	= RectT.anchoredPosition;

			CG.alpha = 1f;

			letterText.gameObject.SetActive(true);

			isHintDisplayed = false;

			SetSelected(false, false);
		}

		public void SetSelected(bool isSelected, bool justSelected)
		{
			Color bkgColor	= isHintDisplayed ? hintBkgColor : normalBkgColor;
			Color textColor	= isHintDisplayed ? hintLetterColor : normalLetterColor;

			bkgImage.color		= isSelected ? selectedBkgColor : bkgColor;
			letterText.color	= isSelected ? selectedLetterColor : textColor;

			// If the tile was just selected (as in, it wasn't selected but now is) then pulse it
			if (justSelected)
			{
				Pulse(scale, pulseAmount, pulseForce, pulseAnimDuration);
			}
		}

		public void ShowHint(bool animate = true)
		{
			isHintDisplayed = true;

			if (animate)
			{
				UIAnimation.Color(bkgImage, hintBkgColor, hintAnimDuration).Play();
				UIAnimation.Color(letterText, hintLetterColor, hintAnimDuration).Play();
			}
			else
			{
				bkgImage.color		= hintBkgColor;
				letterText.color	= hintLetterColor;
			}
		}

		public void HideHint(bool animate = true)
		{
			isHintDisplayed = false;

			if (animate)
			{
				UIAnimation.Color(bkgImage, normalBkgColor, hintAnimDuration).Play();
				UIAnimation.Color(letterText, normalLetterColor, hintAnimDuration).Play();
			}
			else
			{
				bkgImage.color		= normalBkgColor;
				letterText.color	= normalLetterColor;
			}
		}

		public void Shake()
		{
			Shake(position.x, shakeAmount, shakeForce, shakeAnimDuration);
		}

		public void Twist()
		{
			Twist(twistAmount, twistForce, twistAnimDuration);
		}

		public void Remove(System.Action<GameObject> animFinished)
		{
			letterText.gameObject.SetActive(false);

			UIAnimation anim = UIAnimation.Alpha(gameObject, 0f, fadeAnimDuration);

			anim.style = UIAnimation.Style.EaseOut;
			anim.OnAnimationFinished += animFinished;

			anim.Play();
		}

		public void Move(Vector2 toPos, System.Action<GameObject> animFinished = null)
		{
			// Move the x/y position to 0
			UIAnimation.PositionX(RectT, toPos.x, moveAnimDuration).Play();

			UIAnimation anim = UIAnimation.PositionY(RectT, toPos.y, moveAnimDuration);

			if (animFinished != null)
			{
				anim.OnAnimationFinished += animFinished;
			}

			anim.Play();

			position = toPos;
		}

		#endregion

		#region Private Methods

		private void Initialize()
		{
			initialized = true;

			ThemeManager.Instance.Register(this);

			SetColors();
		}

		private void SetColors()
		{
			SetColor(normalBkgColorThemeId, ref normalBkgColor);
			SetColor(normalLetterColorThemeId, ref normalLetterColor);
			SetColor(selectedBkgColorThemeId, ref selectedBkgColor);
			SetColor(selectedLetterColorThemeId, ref selectedLetterColor);
			SetColor(hintBkgColorThemeId, ref hintBkgColor);
			SetColor(hintLetterColorThemeId, ref hintLetterColor);
		}

		private void SetColor(string id, ref Color color)
		{
			if (ThemeManager.Exists() && ThemeManager.Instance.Enabled)
			{
				ThemeManager.ThemeItem	themeItem;
				ThemeManager.ItemId		itemId;

				if (ThemeManager.Instance.GetThemeItem(id, out themeItem, out itemId))
				{
					if (itemId.type != ThemeManager.Type.Color)
					{
						Debug.LogErrorFormat("[GridLettTile] The theme id \"{0}\" is set to type \"{1}\" but we were expecting it to be \"{2}\"", id, itemId.type, ThemeManager.Type.Color);

						return;
					}

					color = themeItem.color;
				}
				else
				{
					Debug.LogErrorFormat("[GridLettTile] Could not find theme id \"{0}\"", id);
				}
			}
			else
			{
				color = Color.white;
			}
		}

		#endregion
	}
}
