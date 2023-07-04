using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public abstract class BaseState
    {
        protected PlayerState playerState;
        
        protected BaseState(PlayerState playerState) 
        {
            this.playerState=playerState;
        }
        public abstract void OnStateEnter();
        public abstract void OnStateUpdate();
        public abstract void OnStateExit();
    }

}
