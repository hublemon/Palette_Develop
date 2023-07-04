using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public class MonsterState : MonoBehaviour
    {
        private enum State
        {
            IDLE,
            AIM,
            ATTACK,
            DIE
        }
        private State currentState;
        private MonsterFSM stateMachine;
        private MonsterController monsterController;

        private void Start()
        {
            currentState = State.IDLE;
            stateMachine = new MonsterFSM(new MonsterIdleState(this));
            monsterController=GetComponent<MonsterController>();    
        }

        private void Update()
        {
            switch (currentState)
            {
                case State.IDLE:
                    UpdateState(currentState);
                    if (monsterController.findPlayer)
                    {
                        ChangeState(State.AIM);
                        monsterController.FindPlayer();
                    }
                    if (monsterController.isDieing)
                    {
                        ChangeState(State.DIE);
                    }
                    break;
                case State.AIM:
                    UpdateState(currentState);
                    if (!monsterController.findPlayer)
                    {
                        ChangeState(State.IDLE);
                        monsterController.Idle();
                    }
                    if (monsterController.canAttackPlayer)
                    {
                        ChangeState(State.ATTACK);
                    }
                    if (monsterController.isDieing)
                    {
                        ChangeState(State.DIE);
                    }
                    break;
                case State.ATTACK:
                    UpdateState(currentState);
                    if (!monsterController.canAttackPlayer)
                        ChangeState(State.AIM);
                    if (monsterController.isDieing)
                    {
                        ChangeState(State.DIE);
                    }
                    break;
                case State.DIE:
                    UpdateState(currentState);
                    break;
            }
        }

        private void ChangeState(State nextState)
        {
            currentState = nextState;
            switch (currentState)
            {
                case State.IDLE:
                    stateMachine.ChangeState(new MonsterIdleState(this)); break;
                case State.AIM:
                    stateMachine.ChangeState(new MonsterAimState(this)); break;
                case State.ATTACK:
                    stateMachine.ChangeState(new MonsterAttackState(this)); break;
                case State.DIE:
                    stateMachine.ChangeState(new MonsterDieState(this)); break;
            }
        }

        private void UpdateState(State currentState)
        {
            switch (currentState)
            {
                case State.IDLE:
                    monsterController.Idle();
                    break;
                case State.AIM:
                        monsterController.FindPlayer();
                    break;
                case State.ATTACK:
                    if (monsterController.canAttackPlayer)
                    {
                        monsterController.FindPlayer();
                        monsterController.OnAttack();
                    }
                    break;
                case State.DIE:
                    monsterController.OnDead();
                    break;
            }
        }
    }
}

