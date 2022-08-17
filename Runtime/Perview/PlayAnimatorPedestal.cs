using SecondLive.Sdk.Sapce;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SecondLive.Maker
{
    public class PlayAnimatorPedestal : MonoBehaviour
    {

        private SL_AnimatorPedestal m_AnimatorPedestal;
        private RuntimeAnimatorController m_DefalutAnimatorController;
        private FirstPersonController m_FirstPersonController;
        private ThirdPersonController m_ThirdPersonController;
        private SwitchController m_SwitchController;
        private Animator m_Animator;
        private StarterAssetsInputs _input;

        // Start is called before the first frame update
        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_DefalutAnimatorController = m_Animator.runtimeAnimatorController;

            _input = GetComponent<StarterAssetsInputs>();
            m_FirstPersonController = GetComponent<FirstPersonController>();
            m_ThirdPersonController = GetComponent<ThirdPersonController>();
            m_SwitchController = GetComponent<SwitchController>();
        }
        
        void Update()
        {
            if (_input.fire != Vector2.zero)
            {
                Ray ray = Camera.main.ScreenPointToRay(_input.fire);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 20))
                {
                    var pedestal =  hitInfo.transform.gameObject.GetComponent<SL_AnimatorPedestal>();
                    if(pedestal != null)AnimatorPedestal = pedestal;
                }
                _input.fire = Vector2.zero;
            }
            else if (_input.move != Vector2.zero && AnimatorPedestal != null)
            {
                AnimatorPedestal = null;
            }
        }

        public SL_AnimatorPedestal AnimatorPedestal
        {
            get => m_AnimatorPedestal;
            set
            {
                if (value == m_AnimatorPedestal)
                    return;

                if (value == null)
                {
                    m_Animator.runtimeAnimatorController = m_DefalutAnimatorController;
                    if(m_AnimatorPedestal != null)
                    {
                        transform.position = m_AnimatorPedestal.exitPlayerLocation.position;
                        transform.rotation = m_AnimatorPedestal.exitPlayerLocation.rotation;
                    }
                    m_AnimatorPedestal = null;

                    if (m_SwitchController.IsThirdPersonController)
                        m_ThirdPersonController.enabled = true;
                    else
                        m_FirstPersonController.enabled = true;
                }
                else
                {
                    m_AnimatorPedestal = value;
                    transform.position = m_AnimatorPedestal.enterPlayerLocation.position;
                    transform.rotation = m_AnimatorPedestal.enterPlayerLocation.rotation;
                    m_Animator.runtimeAnimatorController = value.animatorController;

                    m_FirstPersonController.enabled = false;
                    m_ThirdPersonController.enabled = false;
                }
                
            }
        }
    }
}
