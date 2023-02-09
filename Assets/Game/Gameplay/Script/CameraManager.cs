using UnityEngine;

namespace Funzilla
{
    public class CameraManager : MonoBehaviour

    {
        [SerializeField] private GameObject ball;
        [SerializeField] private GameObject winDisk;
        private Vector3 _collidePosition;
        public float heightGain = 2f;
        private void Start()
        {
            _collidePosition = transform.position;
        }
        private void Update()
        {
            var camPosition = transform.position;
            if (!( camPosition.y - heightGain >= ball.transform.position.y) || !(camPosition.y - heightGain * 4 >= winDisk.transform.position.y)) return;
            var target = new Vector3( camPosition.x, ball.transform.position.y + heightGain,  camPosition.z);
            camPosition = Vector3.Lerp(_collidePosition, target , Time.time * 0.5f);
            transform.position =  camPosition; 
        }
    }
}