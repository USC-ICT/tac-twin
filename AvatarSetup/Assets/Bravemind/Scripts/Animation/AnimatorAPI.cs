using UnityEngine;
using System;

public class AnimatorAPI : MonoBehaviour
{
    public class AnimationState
    {
        private Type          m_animationStateType;
        private string        m_animationStateName;

        public AnimationState(Type animationStateType, string animationStateName)
        {
            m_animationStateType = animationStateType;
            m_animationStateName = animationStateName;
        }

        public Type   AnimationStateType { get { return m_animationStateType; } }
        public string AnimationStateName { get { return m_animationStateName; } }
    }

    public virtual void SetState(int baseState, int state) {}

    public virtual void StartMoving() {}
    public virtual void StopMoving()  {}

}
