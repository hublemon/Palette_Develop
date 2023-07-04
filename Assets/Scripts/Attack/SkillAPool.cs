using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public class SkillAPool : MonoBehaviour
    {
        public static SkillAPool SkillAInstance;

        [Header("AttackEffect")]
        [SerializeField] private GameObject skillAEffectPrefab;

        [Header("EffectPool")]
        [SerializeField] private float initCount;

        Queue<GameObject> skillAPoolingQueue = new Queue<GameObject>();

        private void Awake()
        {
            if (SkillAInstance == null)
            {
                SkillAInstance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
                DestroyImmediate(this.gameObject);
        }

        private void InitialIze(int initCount)
        {
            for (int i = 0; i < initCount; i++)
            {
                skillAPoolingQueue.Enqueue(CreateNewAttack());
            }

        }
        private GameObject CreateNewAttack()
        {
            GameObject newAttack = Instantiate(skillAEffectPrefab);
            newAttack.SetActive(false);
            newAttack.transform.SetParent(transform);
            return newAttack;
        }

        public static GameObject GetAttackEffect()
        {
            if (SkillAInstance.skillAPoolingQueue.Count > 0)
            {
                GameObject attack = SkillAInstance.skillAPoolingQueue.Dequeue();
                attack.SetActive(true);
                attack.transform.SetParent(null);
                return attack;
            }
            else
            {
                GameObject newAttack = SkillAInstance.CreateNewAttack();
                newAttack.SetActive(true);
                newAttack.transform.SetParent(null);
                return newAttack;
            }
        }

        public static void ReturnAttackEffect(GameObject attack)
        {
            attack.gameObject.SetActive(false);
            attack.transform.SetParent(SkillAInstance.transform);
            SkillAInstance.skillAPoolingQueue.Enqueue(attack);
        }
    }
}

