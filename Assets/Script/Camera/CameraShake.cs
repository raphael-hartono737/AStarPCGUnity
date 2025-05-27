using UnityEngine;
using Cinemachine;

namespace MyGame.CameraSystem
{


    public class CameraShake : MonoBehaviour
    {
        public static CameraShake instance;
        public float shakeDuration = 0.2f; // Duration of the shake
        public float shakeAmplitude = 1f;  // How much the camera shakes
        public float shakeFrequency = 5f;  // Speed of the shake

        private CinemachineVirtualCamera virtualCamera;
        private CinemachineBasicMultiChannelPerlin noise;
        private float shakeTimer = 0f;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Get the virtual camera component
            virtualCamera = GetComponent<CinemachineVirtualCamera>();

            // Get the noise component from the virtual camera
            if (virtualCamera != null)
            {
                noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        void Update()
        {
            // Check if the shake timer is active
            if (shakeTimer > 0)
            {
                // Reduce the timer over time
                shakeTimer -= Time.deltaTime;

                // Apply the shake amplitude while the timer is active
                if (noise != null)
                {
                    noise.m_AmplitudeGain = shakeAmplitude;
                    noise.m_FrequencyGain = shakeFrequency;
                }
            }
            else
            {
                // Reset the shake amplitude once the timer runs out
                if (noise != null)
                {
                    noise.m_AmplitudeGain = 0f;
                }
            }
        }

        // Call this function to trigger the shake effect
        public void TriggerShake()
        {
            shakeTimer = shakeDuration;
        }
    }
}