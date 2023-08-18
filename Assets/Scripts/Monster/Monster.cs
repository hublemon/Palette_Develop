using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public class Monster : MonoBehaviour
    {
        [Header("Monster Stat")]
        [SerializeField] protected float maxHP;
        [SerializeField] protected float currentHP;
        [SerializeField] protected float attack;
        [SerializeField] protected float armor;

        public float MaxHP { get { return maxHP; } }
        public float CurrentHP { get { return currentHP; } }
        public float Attack { get { return attack; } }

        public void OnUpdateStat(float maxHP, float currentHP)
        {
            this.maxHP = maxHP;
            this.currentHP = currentHP;
        }
    }
}
