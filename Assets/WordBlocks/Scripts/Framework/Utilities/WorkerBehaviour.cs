using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BizzyBeeGames
{
	public class WorkerBehaviour : MonoBehaviour
	{
		#region Inspector Variables

		#endregion

		#region Member Variables

		private Worker					worker;
		private System.Action<Worker>	finishedCallback;
		private bool					aborted;

		#endregion

		#region Unity Methods

		private void Update()
		{
			if (worker != null && worker.Stopped)
			{
				if (!aborted)
				{
					finishedCallback(worker);
				}

				worker = null;

				Destroy(gameObject);
			}
		}

		#endregion

		#region Public Methods

		public static WorkerBehaviour StartWorker(Worker worker, System.Action<Worker> finishedCallback)
		{
			GameObject		obj				= new GameObject("worker");
			WorkerBehaviour	workerBehaviour	= obj.AddComponent<WorkerBehaviour>();

			workerBehaviour.Run(worker, finishedCallback);

			return workerBehaviour;
		}

		public void Run(Worker worker, System.Action<Worker> finishedCallback)
		{
			this.worker				= worker;
			this.finishedCallback	= finishedCallback;

			worker.StartWorker();
		}

		public void Abort()
		{
			aborted = true;
			worker.Stop();
		}

		#endregion
	}
}
