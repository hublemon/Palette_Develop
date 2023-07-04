using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Palette
{
    public class AttackController : MonoBehaviour
    {
        private Animator animator;

        //private
        private Ray ray;
        private RaycastHit hit;
        private Vector3 screenCenter;

        [Header("Attack Settings")]
        [SerializeField] private float attackDamage = 20f;
        [SerializeField] private float skillADamage = 45f;
        [SerializeField] private float skillBDamage = 40f;
        [Space]
        [SerializeField] private float attackDistance = 75f;
        [SerializeField] private float skillADistance = 150f;
        [SerializeField] private float skillBDistance = 150f;

        [HideInInspector] public bool isAttacking=false;
        [HideInInspector] public bool isSkillA=false;
        [HideInInspector] public bool isSkillB=false;

        private void Start()
        {
            animator = GetComponent<Animator>();
            screenCenter = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
        }
        void DrawArrow()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                isAttacking = true;
                var effect = AttackPool.GetAttackEffect();
                FireAttack(effect);
                StartCoroutine(Return(effect));
            }
            else isAttacking = false;
            
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_A"))
            {
                isSkillA = true;
                var effect = SkillAPool.GetAttackEffect();
                FireAttack(effect);
                StartCoroutine(Return(effect));
            }
            else isSkillA = false;

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_B"))
            {
                isSkillB = true;
                var effect = SkillBPool.GetAttackEffect();
                FireAttack(effect);
                StartCoroutine(Return(effect));
            }
            else isSkillB = false;  
        }

        private void FireAttack(GameObject effect)
        {
            ray=Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 50f, LayerMask.GetMask("Enemy")))
            {
                effect.transform.position = hit.point;
                MonsterController monster=hit.collider.gameObject.GetComponent<MonsterController>();
                if (effect.tag == "Attack")
                    monster.OnDamage(attackDamage);
                else if(effect.tag =="SkillA")
                    monster.OnDamage(skillADamage);
                else if(effect.tag =="SkillB")
                    monster.OnDamage(skillBDamage);
            }
            else if (Physics.Raycast(ray, out hit, 50f, LayerMask.GetMask("Environment")))
            {
                effect.transform.position = hit.point;
            }
            else
            {

                Vector3 endPoint = ray.GetPoint(50f);

                effect.transform.position = endPoint;
            }
        }

        IEnumerator Return(GameObject effect)
        {
            yield return new WaitForSeconds(2.5f);

            if (isAttacking) AttackPool.ReturnAttackEffect(effect);
            if (isSkillA) SkillAPool.ReturnAttackEffect(effect);
            if (isSkillB) SkillBPool.ReturnAttackEffect(effect);
        }
    }
}
