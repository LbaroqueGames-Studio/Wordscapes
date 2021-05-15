using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BizzyBeeGames.WordBlocks
{
	public class BubbleImage : SpawnObject
	{
		#region Inspector Variables

		[SerializeField] private float minSize;
		[SerializeField] private float maxSize;
		[SerializeField] private float minDuration;
		[SerializeField] private float maxDuration;
		[SerializeField] private AnimationCurve floatAnimCurve;
		[SerializeField] private AnimationCurve swayAnimCurve;

		#endregion

		#region Public Methods

		public override void Spawned()
		{
			float size		= Random.Range(minSize, maxSize);
			float duration	= Mathf.Lerp(maxDuration, minDuration, size / maxSize);

			RectT.sizeDelta = new Vector2(size, size);

			UIAnimation anim = UIAnimation.PositionY(RectT, ParentRectT.rect.height / 2f + size, duration);

			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= floatAnimCurve;

			anim.OnAnimationFinished += (GameObject obj) => { Die(); };

			anim.Play();

			anim = UIAnimation.PositionX(RectT, RectT.anchoredPosition.x - size, RectT.anchoredPosition.x + size, 2);

			anim.loopType		= UIAnimation.LoopType.Reverse;
			anim.style			= UIAnimation.Style.Custom;
			anim.animationCurve	= swayAnimCurve;

			anim.Play();
		}

		#endregion
	}
}
