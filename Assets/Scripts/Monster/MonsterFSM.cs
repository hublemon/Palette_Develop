using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public class MonsterFSM
    {
        private MonsterBaseState currentState;
        public MonsterFSM(MonsterBaseState initState) 
        {
            currentState = initState;
            ChangeState(currentState);
        }

        public void ChangeState(MonsterBaseState nextState)
        {
            if (nextState == currentState)
                return;
            if (currentState != null)
                currentState.OnExitState();
            currentState = nextState;
            currentState.OnEnterState();
        }

        public void UpdateState()
        {
            if(currentState != null)    
                currentState.OnUpdateState();
        }

    }

}
