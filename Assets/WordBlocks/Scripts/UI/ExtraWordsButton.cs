using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BizzyBeeGames.WordBlocks
{
	public class ExtraWordsButton : UIMonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private ProgressRing	progressRing		= null;
		[SerializeField] private float			ringAnimDuration	= 0;
		[Space]
		[SerializeField] private int	pulseAmount					= 0;
		[SerializeField] private float 	pulseForce					= 0;
		[SerializeField] private float	pulseAnimDuration			= 0;

		#endregion

		#region Member Variables

		private float currentPercent;

		private bool	isAnimating;
		private float	timer;
		private float	prevPercent;

		#endregion

		#region Unity Methods

		private void Start()
		{
			currentPercent = GetCurrentPercentage();

			progressRing.SetProgress(currentPercent);

			GameManager.Instance.OnExtraWordFound += ExtraWordFound;
		}

		private void Update()
		{
			if (isAnimating)
			{
				timer += Time.deltaTime;

				if (timer >= ringAnimDuration)
				{
					isAnimating = false;
				}

				progressRing.SetProgress(Mathf.Lerp(prevPercent, currentPercent, timer / ringAnimDuration));

				if (!isAnimating && currentPercent == 1)
				{
					prevPercent		= 1;
					currentPercent	= GetCurrentPercentage();
					isAnimating		= true;
					timer			= 0;
				}
			}
		}

		#endregion

		#region Public Methods

		public void Pulse()
		{
			Pulse(transform.localScale, pulseAmount, pulseForce, pulseAnimDuration);
		}

		#endregion

		#region Private Methods

		private void ExtraWordFound()
		{
			prevPercent		= currentPercent;
			currentPercent	= Mathf.Clamp01(GetCurrentPercentage());
			isAnimating		= true;
			timer			= 0;
		}

		private float GetCurrentPercentage()
		{
			if (GameManager.Instance.ExtraWordsAmount == 0)
			{
				return 0;
			}

			return (float)GameManager.Instance.NumExtraWordLettersFound / (float)GameManager.Instance.ExtraWordsAmount;
		}

		#endregion
	}
}
