using UnityEngine;

namespace Funzilla
{
	public class PostProcess : MonoBehaviour
	{
		[SerializeField] private Shader shader;
		private Material _material;

		private void Start()
		{
			_material = new Material(shader);
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			Graphics.Blit(src, dest, _material);
		}
	}
}