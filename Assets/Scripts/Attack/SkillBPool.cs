using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public class SkillBPool : MonoBehaviour
    {
        public static SkillBPool SkillBInstance;

        [Header("AttackEffect")]
        [SerializeField] private GameObject skillBEffectPrefab;

        [Header("EffectPool")]
        [SerializeField] private float initCount;

        Queue<GameObject> skillBPoolingQueue = new Queue<GameObject>();

        private void Awake()
        {
            if (SkillBInstance == null)
            {
                SkillBInstance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
                DestroyImmediate(this.gameObject);
        }

        private void InitialIze(int initCount)
        {
            for (int i = 0; i < initCount; i++)
            {
                skillBPoolingQueue.Enqueue(CreateNewAttack());
            }

        }
        private GameObject CreateNewAttack()
        {
            GameObject newAttack = Instantiate(skillBEffectPrefab);
            newAttack.SetActive(false);
            newAttack.transform.SetParent(transform);
            return newAttack;
        }

        public static GameObject GetAttackEffect()
        {
            if (SkillBInstance.skillBPoolingQueue.Count > 0)
            {
                GameObject attack = SkillBInstance.skillBPoolingQueue.Dequeue();
                attack.SetActive(true);
                attack.transform.SetParent(null);
                return attack;
            }
            else
            {
                GameObject newAttack = SkillBInstance.CreateNewAttack();
                newAttack.SetActive(true);
                newAttack.transform.SetParent(null);
                return newAttack;
            }
        }

        public static void ReturnAttackEffect(GameObject attack)
        {
            attack.gameObject.SetActive(false);
            attack.transform.SetParent(SkillBInstance.transform);
            SkillBInstance.skillBPoolingQueue.Enqueue(attack);
        }
    }

}