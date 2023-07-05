using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Palette
{
    public class PlayerState : MonoBehaviour
    {
        private enum State
        {
            MOVE,
            AIM,
            ATTACK,
            DIE
        }

        private State currentState;
        private StateMachine stateMachine;
        private PlayerContoller playerController;
        private Animator animator;

        private void Start()
        {
            playerController = GetComponent<PlayerContoller>();  
            animator = GetComponent<Animator>();
            currentState=State.MOVE;
            stateMachine=new StateMachine(new MoveState(this));
        }

        private void Update()
        {
            switch(currentState)
            {
                case State.MOVE:
                    UpdateState(currentState);
                    if (playerController.isAiming)
                    {
                        ChangeState(State.AIM);
                        playerController.OnMove();
                    }
                    if (playerController.isDieing)
                    {
                        ChangeState(State.DIE);
                    }
                    break; 
                case State.AIM:
                    UpdateState(currentState);
                    if (!playerController.isAiming)
                    {
                        ChangeState(State.MOVE);
                        playerController.OnMove();
                    }
                    if (playerController.isAttacking)
                    {
                        ChangeState(State.ATTACK);
                    }
                    if (playerController.isDieing)
                    {
                        ChangeState(State.DIE);
                    }
                    break;
                case State.ATTACK:
                    UpdateState(currentState);
                    if ( playerController.isAiming)
                        ChangeState(State.AIM);
                    else if (!playerController.isAiming)
                    {
                        playerController.OnMove();
                        ChangeState(State.MOVE);
                    }
                    if (playerController.isDieing)
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
            currentState= nextState;
            switch (currentState)
            {
                case State.MOVE:
                    stateMachine.ChangeState(new MoveState(this)); break;
                case State.AIM:
                    stateMachine.ChangeState(new AimState(this)); break;    
                case State.ATTACK:
                    stateMachine.ChangeState(new AttackState(this)); break;
                case State.DIE:
                    stateMachine.ChangeState(new DieState(this)); break;   
            }
        }

        private void UpdateState(State currentState)
        {
            switch (currentState)
            {
                case State.MOVE:
                    if (playerController.isGrounded)
                        playerController.OnMove();
                    playerController.OnJump();
                        break;
                case State.AIM:
                    if (playerController.isAiming)
                        playerController.OnMove();
                    if(!playerController.isAttacking)
                        playerController.OnAttack();
                    break;
                case State.ATTACK:
                    if (playerController.isAttacking)
                        playerController.OnAttack();
                    break;
                case State.DIE:
                    playerController.OnDead();
                    break;
            }
        }
    }
}

