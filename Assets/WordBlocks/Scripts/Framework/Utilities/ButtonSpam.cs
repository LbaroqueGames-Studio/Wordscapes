using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BizzyBeeGames
{
	public class ButtonSpam : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private List<GameObject>	screenObjs				= null;
		[SerializeField] private float				startDelay				= 0f;
		[SerializeField] private float				timeBetweenButtonClicks	= 0f;
		[SerializeField] private int				seed					= 0;

		#endregion

		#region Member Variables

		private bool running;

		#endregion

		#region Unity Methods

		private void Start()
		{
			Begin();		
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.X))
			{
				running = false;
			}
		}

		#endregion

		#region Public Methods

		public void Begin()
		{
			running = true;

			int seedToUse = seed;

			if (seedToUse == 0)
			{
				seedToUse = Random.Range(0, int.MaxValue);

				Debug.Log("Seed: " + seedToUse);
			}

			Random.InitState(seedToUse);

			StartCoroutine(ClickButtons());
		}

		#endregion

		#region Protected Methods

		#endregion

		#region Private Methods

		private IEnumerator ClickButtons()
		{
			yield return new WaitForSeconds(startDelay);

			while (running)
			{
				ClickRandomButton();

				yield return new WaitForSeconds(timeBetweenButtonClicks);
			}
		}

		private void ClickRandomButton()
		{
			string log = "";

			List<GameObject> interactableScreens = new List<GameObject>();

			log += "=== Interactable objects:";

			if (PopupManager.Instance.activePopups.Count > 0)
			{
				log += "\n" + PopupManager.Instance.activePopups[PopupManager.Instance.activePopups.Count - 1].gameObject.name;

				interactableScreens.Add(PopupManager.Instance.activePopups[PopupManager.Instance.activePopups.Count - 1].gameObject);
			}
			else
			{
				for (int i = 0; i < screenObjs.Count; i++)
				{
					GameObject screenObj = screenObjs[i];
					CanvasGroup canvasGroup = screenObj.GetComponent<CanvasGroup>();

					if (canvasGroup == null || canvasGroup.interactable)
					{
						log += "\n" + screenObj.name;

						interactableScreens.Add(screenObj);
					}
				}
			}

			log += "\n===";

			if (interactableScreens.Count == 0)
			{
				log += "\nNo interactable screens";
			}
			else
			{
				GameObject screen = interactableScreens[Random.Range(0, interactableScreens.Count)];

				log += "\nClicking button on " + screen.gameObject.name;

				List<Button> buttons = new List<Button>(screen.GetComponentsInChildren<Button>(false));

				for (int i = buttons.Count - 1; i >= 0; i--)
				{
					Button button = buttons[i];

					if (button.gameObject.GetComponent<ButtonSpamIgnore>() != null || !button.interactable)
					{
						buttons.RemoveAt(i);
					}
				}

				if (buttons.Count == 0)
				{
					log += "\nNo buttons to click";
				}
				else
				{
					log += "\nThere are " + buttons.Count + " buttons to choose from";

					Button buttonToClick = buttons[Random.Range(0, buttons.Count)];

					log += "\nClicking button " + buttonToClick.gameObject.name;

					buttonToClick.onClick.Invoke();
				}
			}

			Debug.Log(log);
		}

		#endregion
	}
}
