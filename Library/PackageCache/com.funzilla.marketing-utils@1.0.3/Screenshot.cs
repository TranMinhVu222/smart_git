using UnityEngine;

namespace Funzilla
{
	public class Screenshot : MonoBehaviour
	{
		[SerializeField] private string folderPath = "";
		[SerializeField] private KeyCode captureKey = KeyCode.Space;

		private void Update()
		{
			if (Input.GetKeyDown(captureKey))
			{
				ScreenCapture.CaptureScreenshot(folderPath + "/" + Time.time + ".png");
			}
		}
	}
}
