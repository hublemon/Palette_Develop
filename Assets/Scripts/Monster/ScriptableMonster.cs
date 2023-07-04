using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    [CreateAssetMenu(fileName ="Scriptable/MonsterData",menuName ="MonsterData")]
    public class ScriptableMonster : ScriptableObject
    {
        public float hp = 50f;
        public float HP { get { return hp; } set { hp = value; } }

        public float attackDamage = 15f;
        public float AttackDamage { get { return attackDamage; } set {  attackDamage = value; } }

        public float attackingTime = 3f;
        public float AttackingTime { get { return attackingTime; } set {  attackingTime = value; } }

       

    }
}
