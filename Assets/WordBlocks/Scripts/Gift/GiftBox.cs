using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BizzyBeeGames.WordBlocks
{
	public class GiftBox : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private RectTransform	giftBase;
		[SerializeField] private RectTransform	giftTop;
		[SerializeField] private RectTransform	giftContainer;
		[SerializeField] private Image			giftImage;
		[SerializeField] private Text			giftAmountText;

		#endregion

		#region Properties

		public RectTransform CoinMarker { get { return giftImage.rectTransform; } }

		#endregion

		#region Public Methods

		public IEnumerator PlayOpenAnimation(Sprite giftSprite, int giftAmount)
		{
			giftImage.sprite	= giftSprite;
			giftAmountText.text	= "x" + giftAmount.ToString();

			UIAnimation anim;

			// Move the top up
			anim		= UIAnimation.PositionY(giftTop, 100f, 0.5f);
			anim.style	= UIAnimation.Style.EaseOut;
			anim.Play();

			// Fade out the top
			anim		= UIAnimation.Alpha(giftTop.gameObject, 0f, 0.5f);
			anim.style	= UIAnimation.Style.EaseOut;
			anim.OnAnimationFinished += (GameObject obj) => { obj.SetActive(false); };
			anim.Play();

			//yield return new WaitForSeconds(0.15f);
			while (anim.IsPlaying)
			{
				yield return null;
			}

			// Move the gift up
			anim		= UIAnimation.PositionY(giftContainer, 300f, 0.5f);
			anim.style	= UIAnimation.Style.EaseOut;
			anim.Play();
			
			// Fade in the gift
			anim		= UIAnimation.Alpha(giftContainer.gameObject, 0f, 1f, 0.5f);
			anim.style	= UIAnimation.Style.EaseOut;
			anim.Play();

			yield return new WaitForSeconds(0.15f);

			// Fade out the base
			anim		= UIAnimation.Alpha(giftBase.gameObject, 0f, 0.5f);
			anim.style	= UIAnimation.Style.EaseOut;
			anim.OnAnimationFinished += (GameObject obj) => { obj.SetActive(false); };
			anim.Play();

			while (anim.IsPlaying)
			{
				yield return null;
			}
		}

		#endregion
	}
}
