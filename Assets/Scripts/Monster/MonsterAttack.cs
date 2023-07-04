using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public class MonsterAttack : MonoBehaviour
    {
        private ParticleSystem particle;
        private MonsterController monsterController;
        private Transform rootParent;

        void Start()
        {
            particle=GetComponent<ParticleSystem>();
            rootParent = this.transform.parent;
            while(rootParent.parent != null)
            {
                rootParent = rootParent.parent;
            }
            monsterController=rootParent.gameObject.GetComponent<MonsterController>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnParticleCollision(GameObject other)
        {
            if (other.tag == "Player")
            {
                Debug.Log("공격에 닿았다");
                if (monsterController.attaking == 0)
                    monsterController.attaking = Time.deltaTime;
                monsterController.playerDamage = true;
            }
        }
    }
}

