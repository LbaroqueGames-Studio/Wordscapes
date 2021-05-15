using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BizzyBeeGames.WordBlocks
{
	public class SelectedWordHandler : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private CanvasGroup	container			= null;
		[SerializeField] private Text			selectedLettersText	= null; 
		[SerializeField] private float			shakeForce			= 10; 
		[SerializeField] private float			shakeAmount			= 3; 
		[SerializeField] private float			shakeAnimDuration	= 0.5f; 
		[SerializeField] private float			fadeAnimDuration	= 0.5f; 

		#endregion

		#region Public Methods

		public void Initialize()
		{
			container.alpha = 0f;
			container.gameObject.SetActive(true);
		}

		public void SetSelectedLetters(string selectedLetters)
		{
			StopAllCoroutines();

			UIAnimation.DestroyAllAnimations(container.gameObject);

			container.alpha = 1f;
			(container.transform as RectTransform).anchoredPosition = Vector2.zero;

			selectedLettersText.text = selectedLetters;
		}

		public void FadeOut()
		{
			UIAnimation.Alpha(container.gameObject, 0f, fadeAnimDuration).Play();
		}

		public void ShakeAndFadeOut()
		{
			StartCoroutine(StartShake());
		}

		public void Clear()
		{
			UIAnimation.DestroyAllAnimations(container.gameObject);
			container.alpha = 0f;
		}

		#endregion

		#region Private Methods

		private IEnumerator StartShake()
		{
			for (int i = 0; i < shakeAmount; i++)
			{
				if (i % 2 == 0)
				{
					ShakeLeft(shakeAnimDuration);
				}
				else
				{
					ShakeRight(shakeAnimDuration);
				}

				yield return new WaitForSeconds(shakeAnimDuration);
			}

			// Move it back to the middle
			UIAnimation.PositionX(container.transform as RectTransform, 0, shakeAnimDuration).Play();

			yield return new WaitForSeconds(shakeAnimDuration);

			FadeOut();
		}

		private void ShakeLeft(float animDuration)
		{
			UIAnimation.PositionX(container.transform as RectTransform, -shakeForce, animDuration).Play();
		}

		private void ShakeRight(float animDuration)
		{
			UIAnimation.PositionX(container.transform as RectTransform, shakeForce, animDuration).Play();
		}

		#endregion
	}
}
