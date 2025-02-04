﻿using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>
    /// Recieve messages from the Animator State Machine Behaviours using MessageBehaviour
    /// </summary>
    public interface IAnimatorListener
    {
        /// <summary> Recieve messages from the Animator State Machine Behaviours </summary>
        /// <param name="message">The name of the method</param>
        /// <param name="value">the parameter</param>
        void OnAnimatorBehaviourMessage(string message, object value);

        /*
        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);
        }
        */
    }

    //public interface IAnimatorParameters
    //{
    //    void SetAnimParameter(int hash, int value);

    //    /// <summary>Set a float on the Animator</summary>
    //    void SetAnimParameter(int hash, float value);

    //    /// <summary>Set a Bool on the Animator</summary>
    //    void SetAnimParameter(int hash, bool value);

    //    /// <summary>Set a Trigger to the Animator</summary>
    //    void SetAnimParameter(int hash);
    //}

    public class MessagesBehavior : StateMachineBehaviour
    {
        public bool UseSendMessage;

        public MesssageItem[] onEnterMessage;   //Store messages to send it when Enter the animation State
        public MesssageItem[] onExitMessage;    //Store messages to send it when Exit  the animation State
        public MesssageItem[] onTimeMessage;    //Store messages to send on a specific time  in the animation State

        IAnimatorListener[] listeners;         //To all the MonoBehavious that Have this 


        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            listeners = animator.GetComponents<IAnimatorListener>();


            foreach (MesssageItem ontimeM in onTimeMessage)  //Set all the messages Ontime Sent = false when start
            {
                ontimeM.sent = false;
            }

            foreach (MesssageItem onEnterM in onEnterMessage)
            {
                if (onEnterM.Active && !string.IsNullOrEmpty(onEnterM.message))
                {
                    if (UseSendMessage)
                        DeliverMessage(onEnterM, animator);
                    else
                        foreach (var item in listeners) DeliverListener(onEnterM, item);
                }
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (MesssageItem onExitM in onExitMessage)
            {
                if (onExitM.Active && !string.IsNullOrEmpty(onExitM.message))
                {
                    if (UseSendMessage)
                        DeliverMessage(onExitM, animator);
                    else
                        foreach (var item in listeners) DeliverListener(onExitM, item);
                }
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.fullPathHash == animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash) return; //means is transitioning to itself

            foreach (MesssageItem onTimeM in onTimeMessage)
            {
                if (onTimeM.Active && onTimeM.message != string.Empty)
                {
                    // float stateTime = stateInfo.loop ? stateInfo.normalizedTime % 1 : stateInfo.normalizedTime;
                    float stateTime = stateInfo.normalizedTime % 1;

                    if (!onTimeM.sent && (stateTime >= onTimeM.time))
                    {
                        onTimeM.sent = true;

                      //  Debug.Log(onTimeM.message + ": "+stateTime);

                        if (UseSendMessage)
                            DeliverMessage(onTimeM, animator);
                        else
                            foreach (var item in listeners) DeliverListener(onTimeM, item);
                    }
                }
            }
        }

        /// <summary>
        /// Using Message to the Monovehaviours asociated to this animator delivery with Send Message
        /// </summary>
        void DeliverMessage(MesssageItem m, Animator anim)
        {
            switch (m.typeM)
            {
                case TypeMessage.Bool:
                    anim.SendMessage(m.message, m.boolValue, SendMessageOptions.DontRequireReceiver);
                    break;
                case TypeMessage.Int:
                    anim.SendMessage(m.message, m.intValue, SendMessageOptions.DontRequireReceiver);
                    break;
                case TypeMessage.Float:
                    anim.SendMessage(m.message, m.floatValue, SendMessageOptions.DontRequireReceiver);
                    break;
                case TypeMessage.String:
                    anim.SendMessage(m.message, m.stringValue, SendMessageOptions.DontRequireReceiver);
                    break;
                case TypeMessage.Void:
                    anim.SendMessage(m.message, SendMessageOptions.DontRequireReceiver);
                    break;
                case TypeMessage.IntVar:
                    anim.SendMessage(m.message,(int) m.intVarValue, SendMessageOptions.DontRequireReceiver);
                    break;
                case TypeMessage.Transform:
                    anim.SendMessage(m.message, m.transformValue, SendMessageOptions.DontRequireReceiver);
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// Send messages to all scripts with IBehaviourListener to this animator 
        /// </summary>
        void DeliverListener(MesssageItem m, IAnimatorListener listener)
        {
            switch (m.typeM)
            {
                case TypeMessage.Bool:
                    listener.OnAnimatorBehaviourMessage(m.message, m.boolValue);
                    break;
                case TypeMessage.Int:
                    listener.OnAnimatorBehaviourMessage(m.message, m.intValue);
                    break;
                case TypeMessage.Float:
                    listener.OnAnimatorBehaviourMessage(m.message, m.floatValue);
                    break;
                case TypeMessage.String:
                    listener.OnAnimatorBehaviourMessage(m.message, m.stringValue);
                    break;
                case TypeMessage.Void:
                    listener.OnAnimatorBehaviourMessage(m.message, null);
                    break;
                case TypeMessage.IntVar:
                    listener.OnAnimatorBehaviourMessage(m.message, (int) m.intVarValue);
                    break;
                case TypeMessage.Transform:
                    listener.OnAnimatorBehaviourMessage(m.message, m.transformValue);
                    break;
                default:
                    break;
            }
        }
    }
    [System.Serializable]
    public class MesssageItem
    {
        public string message;
        public TypeMessage typeM;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;
        public IntVar intVarValue;
        public Transform transformValue;

        public float time;
        public bool sent;
        public bool Active = true;

        public MesssageItem()
        {
            message = string.Empty;
            Active = true;
        }
    }
}