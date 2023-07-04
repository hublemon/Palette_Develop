using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

namespace Palette
{
    public class MonsterController : MonoBehaviour
    {
        private Monster monster;
        public ScriptableMonster monsterdata;
        public ScriptableMonster MonsterData { set { monsterdata = value; } }

        [Header("Offset")]
        [SerializeField] private float rotationSharpness = 1f;
        [SerializeField] private float scaleOffset = 0.15f;

        [Space]
        [Header("Attack")]
        [SerializeField] private float attackCool = 5f;
        [SerializeField] private float detectDistance = 102f;
        [SerializeField] private float attackDistance = 10f;
        [SerializeField] private GameObject targetEffect;

        //private
        private Quaternion targetRotation;
        private Quaternion newRotation;
        private Quaternion initalRotation;
        private Quaternion idleRotation;
        private float coolTime;
        private float attackDelay=0f;
        private float distance;
        private float localScaleX;
        private PlayerContoller player;
        private bool up=true;
        private bool reverse=false;
        private float attackDmg = 0f;

        [SerializeField]private Material material;
        [SerializeField] private float dieingTime = 3f;

        [HideInInspector] public float attaking = 0f;
        [HideInInspector] public bool isDieing;
        [HideInInspector] public bool findPlayer;
        [HideInInspector] public bool playerDamage=false;
        [HideInInspector] public bool playerGetDamage=false;
        [HideInInspector] public bool canAttackPlayer;
        [HideInInspector] public bool IsAttacking;


        void Start()
        {
            monster=GetComponent<Monster>();
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerContoller>();
            attackDmg = monsterdata.AttackDamage;
            
            attaking = 0f;
            coolTime = attackCool + monsterdata.AttackingTime;

            monster.OnUpdateStat(monsterdata.HP, monsterdata.HP);

            initalRotation = transform.rotation;
            localScaleX=transform.localScale.x;
        }

        public void Idle()
        {
            Quaternion baseRotation = Quaternion.Euler(0, 30, 0);
            Quaternion reverseRotation = Quaternion.Euler(0, -30, 0);
            if (!reverse)
            {
                idleRotation = initalRotation * baseRotation;
                if (transform.rotation == idleRotation)
                    reverse = true;
            }
            else
            {
                idleRotation = initalRotation * reverseRotation;
                if (transform.rotation == idleRotation)
                    reverse = false;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, idleRotation, Time.deltaTime * rotationSharpness);
        }

        private void CalculateDistance()
        {
            distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance <= detectDistance)
            {
                if (distance <= attackDistance)
                    canAttackPlayer = true;
                else canAttackPlayer = false;
                findPlayer = true;
            }
            else
            {
                canAttackPlayer = false;
                findPlayer = false;
            }
            //Debug.Log(findPlayer);
        }

        public void FindPlayer() 
        {
            //Debug.Log("적이 나를 발견했다");
            if (up)
            {
                if (transform.localScale.x < localScaleX+localScaleX*scaleOffset)
                    transform.localScale += new Vector3(1,0,1)*0.1f*localScaleX/27;
                else
                    up = false;
            }
            else
            {
                if (transform.localScale.x > localScaleX - localScaleX * scaleOffset)
                    transform.localScale -= new Vector3(1, 0, 1)*0.1f * localScaleX / 27;
                else
                    up = true;

            }
            Vector3 targetDirection = player.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness);
            transform.rotation = newRotation;   
        }

        public void OnAttack() 
        {
            if (attackDelay >= coolTime)
            {
                attackDelay = 0f;
                targetEffect.SetActive(true);
                Debug.Log("적이 공격을 시작했다");
                StartCoroutine(SetAttack(monsterdata.AttackingTime));
            }
        }

        IEnumerator SetAttack(float time)
        {
            yield return new WaitForSeconds(time);
            targetEffect.SetActive(false);
        }


        private void AttackCoolCalculate()
        {
            attackDelay += Time.deltaTime;
            if (attaking != 0)
                attaking += Time.deltaTime;
            if(attaking>monsterdata.AttackingTime)
            {
                playerDamage = false;
                playerGetDamage = false;
                attaking = 0;
            }
        }
        private void CheckAlive()
        {
            if (monster.CurrentHP <= 0)
                isDieing = true;
            else
                isDieing = false;
        }

        public void OnDamage(float damage)
        {
            monster.OnUpdateStat(monster.MaxHP, monster.CurrentHP - damage);
            Debug.Log("공격 성공했다"+monster.CurrentHP);
        }

        public void OnDead()
        {
            Debug.Log("적이 사라진다");
            Material[] materials = this.GetComponent<MeshRenderer>().materials;
            if (materials != null)
            {
                for(int t=0; t<materials.Length;t++)
                {
                    materials[t] = material;
                }
                this.GetComponent<MeshRenderer>().materials=materials;
            }
            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.GetComponent<MeshRenderer>() != null)
                {
                    Material[] mats = transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().materials;
                    if (mats != null)
                    {
                        for (int t = 0; t < mats.Length; t++)
                        {
                            mats[t] = material;
                        }
                        transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().materials = mats;
                    }
                }
            }
            Debug.Log("적을 처치했다");
            Destroy(this.gameObject, dieingTime);
        }

        void Update()
        {
            CalculateDistance();
            AttackCoolCalculate();
            if (playerDamage && !playerGetDamage)
            {
                player.OnDamage(monsterdata.AttackDamage);
                playerGetDamage = true;
            }
            CheckAlive();
        }
    }
}

