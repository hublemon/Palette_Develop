using Palette;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Palette
{
    public enum State
    {
        MOVE,
        ATTACK,
        DIE
    }
    public class StateMachine
    {
        private BaseState currentState;
        public StateMachine(BaseState initState) {
            currentState = initState;
            ChangeState(currentState);
        }   

        public void ChangeState(BaseState nextState)
        {
            if (nextState == currentState)
                return;
            if (currentState != null)
                currentState.OnStateExit();
            
            currentState = nextState;
            nextState.OnStateEnter();
        }

        public void UpdateState()
        {
            if (currentState != null)
                currentState.OnStateUpdate();
        }
    }
}


