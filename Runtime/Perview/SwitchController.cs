using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SecondLive.Maker
{
    [RequireComponent(typeof(ThirdPersonController),typeof(FirstPersonController))]
    public class SwitchController : MonoBehaviour
    {
        public Cinemachine.CinemachineVirtualCamera thirdPersonCamera;
        public Cinemachine.CinemachineVirtualCamera firstPersonCamera;

        StarterAssetsInputs inputs;
        ThirdPersonController thirdPersonController;
        FirstPersonController firstPersonController;
        Renderer renderer;

        public bool IsThirdPersonController { get; set; }

        private void Start()
        {
            inputs = GetComponent<StarterAssetsInputs>();
            thirdPersonController = GetComponent<ThirdPersonController>();
            firstPersonController = GetComponent<FirstPersonController>();
            renderer = GetComponentInChildren<SkinnedMeshRenderer>();

            firstPersonController.enabled = false;
            firstPersonCamera.gameObject.SetActive(false);
            IsThirdPersonController = true;
        }

        private void LateUpdate()
        {
            if (inputs.view)
            {
                if (thirdPersonController.enabled)
                {
                    IsThirdPersonController = false;
                    thirdPersonController.enabled = false;
                    thirdPersonCamera.gameObject.SetActive(false);

                    renderer.enabled = false;

                    firstPersonController.enabled = true;
                    firstPersonCamera.gameObject.SetActive(true);
                }
                else
                {
                    IsThirdPersonController = true;
                    thirdPersonController.enabled = true;
                    thirdPersonCamera.gameObject.SetActive(true);

                    renderer.enabled = true;

                    firstPersonController.enabled = false;
                    firstPersonCamera.gameObject.SetActive(false);
                }
                    

                inputs.view = false;
            }
        }
    }
}
