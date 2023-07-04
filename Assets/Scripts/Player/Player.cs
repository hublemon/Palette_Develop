using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public class Player : MonoBehaviour
    {
        private static Player instance;
        public static Player Instance { get { return instance; } }

        public StateMachine stateMachine { get; private set; }
        public Rigidbody rigidbody { get; private set; }
        public Animator animator { get; private set; }
        //public CapsuleCollider capsuleCollider { get; private set; }
        public PlayerInputs inputs { get; private set; }

        //Character Stat
        [Header("Player Stat")]
        [SerializeField] protected float maxHP;
        [SerializeField] protected float currentHP;
        [SerializeField] protected float walkSpeed=2f;
        [SerializeField] protected float runSpeed=4f;
        [SerializeField] protected float sprintSpeed=6f;
        [SerializeField] protected float jumpForce=10f;
        [SerializeField] protected float attack;
        [SerializeField] protected float armor;

        public float MaxHP { get { return maxHP; } }
        public float CurrentHP { get { return currentHP; } }
        public float WalkSpeed { get { return walkSpeed; } }
        public float RunSpeed { get { return runSpeed; } }
        public float SprintSpeed { get { return sprintSpeed; } }
        public float JumpForce { get { return jumpForce; } }
        public float Attack { get { return attack; } }
        public float Armor { get { return armor; } }

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
                rigidbody = GetComponent<Rigidbody>();
                animator = GetComponent<Animator>();
                //capsuleCollider = GetComponent<CapsuleCollider>();
                inputs = GetComponent<PlayerInputs>();
                DontDestroyOnLoad(gameObject);
                return;
            }
            DestroyImmediate(gameObject);
        }

        public void OnUpdateStat(float maxHP, float currentHP,float attack,float armor)
        {
            this.maxHP = maxHP;
            this.currentHP = currentHP;
            this.attack = attack;
            this.armor = armor;
        }

    }

}
