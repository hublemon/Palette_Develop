using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{

    public class AttackPool : MonoBehaviour
    {
        public static AttackPool AttackInstance;

        [Header("AttackEffect")]
        [SerializeField] private GameObject attackEffectPrefab;

        [Header("EffectPool")]
        [SerializeField] private float initCount;

        Queue<GameObject> attackPoolingQueue = new Queue<GameObject>();

        private void Awake()
        {
            if (AttackInstance == null)
            {
                AttackInstance = this; 
                DontDestroyOnLoad(this.gameObject);
            }
            else
                DestroyImmediate(this.gameObject);
        }

        private void InitialIze(int initCount)
        {
            for(int i = 0; i < initCount; i++)
            {
                attackPoolingQueue.Enqueue(CreateNewAttack());
            }

        }
        private GameObject CreateNewAttack()
        {
            GameObject  newAttack = Instantiate(attackEffectPrefab);
            newAttack.SetActive(false);  
            newAttack.transform.SetParent(transform);
            return newAttack;    
        }

        public static GameObject GetAttackEffect()
        {
            if(AttackInstance.attackPoolingQueue.Count > 0)
            {
                GameObject attack=AttackInstance.attackPoolingQueue.Dequeue();
                attack.SetActive(true);
                attack.transform.SetParent(null);
                return attack;
            }
            else
            {
                GameObject newAttack = AttackInstance.CreateNewAttack();
                newAttack.SetActive(true);  
                newAttack.transform.SetParent(null); 
                return newAttack;
            }
        }

        public static void ReturnAttackEffect(GameObject attack)
        {
            attack.gameObject.SetActive(false);
            attack.transform.SetParent(AttackInstance.transform);
            AttackInstance.attackPoolingQueue.Enqueue(attack);
        }

    }
}

