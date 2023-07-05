using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Palette
{
    public class PlayerContoller : MonoBehaviour
    {
        public Player player { get; private set; }

        [Header("MoveSharpness")]
        [SerializeField] private float moveSharpness = 10f;
        [SerializeField] private float rotationSharpness = 10f;

        [Space]
        [Header("SlopCheck")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float maxSlopeAngle=60f;

        [Space]
        [Header("GroundCheck")]
        [SerializeField] private float offSet;
        [SerializeField] LayerMask groundLayer;

        [Space]
        [Header("Attack")]
        [SerializeField] private float attackCool=1.5f;
        [SerializeField] private float skillACool=10f;
        [SerializeField] private float skillBCool=15f;

        [Space]
        [Header("Aim")]
        [SerializeField] private Image aim;
        [SerializeField] Sprite[] aimSprites=new Sprite[3];
        private RectTransform aimRect;

        [Space]
        [Header("HP")]
        [SerializeField] private Image hpBar;
        [SerializeField] private Image hpBarHead;
        private RectTransform hpRect;

        private CameraController cameraContorller;

        //private
        [HideInInspector] public bool isAiming;
        [HideInInspector] public bool sprinting;
        private float strafeParameter;
        private Vector3 strafeParameterXZ;
        private float targetSpeed;
        private float newSpeed;
        private Vector3 newVelocity;
        private Vector3 attackVelocity;
        private Quaternion targetRotation;
        private Quaternion newRotation;
        private Vector3 gravity;
        private Vector3 moveInputVector;
        private Vector3 cameraPlanarDirection;
        private Quaternion cameraPlanarRotation;
        private Vector3 moveInputVectorOriented;

        private const float RAY_DISTANCE = 0.45f;
        private RaycastHit slopeHit;

        [HideInInspector] public bool isJumping;
        [HideInInspector] public bool isFalling;
        [HideInInspector] public bool isGrounded;
        private float checkTime = 0;
        private float jumpDelayTime=1.5f;

        [HideInInspector] public bool isAttacking;
        [HideInInspector] public bool skillA;
        [HideInInspector] public bool skillB;
        [HideInInspector] public bool playingAttack;
        
        private float attackTime=0f;
        private float skillATime=0f;
        private float skillBTime=0f;

        [HideInInspector] public bool isDieing;


        void Start()
        {
            isDieing = false;
            player=GetComponent<Player>();  
            cameraContorller=GetComponent<CameraController>();
            player.rigidbody.angularDrag = 999;
            player.rigidbody.useGravity = true;
            player.animator.applyRootMotion=false;
            player.OnUpdateStat(player.MaxHP, player.MaxHP,player.Attack,player.Armor);

            aimRect = (RectTransform)aim.transform;
            hpRect = (RectTransform)hpBarHead.transform;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.layer == groundLayer)
                isGrounded = true;
            else isGrounded=true;
        }

        private void CaculateVelocity()
        {
            moveInputVector = new Vector3(player.inputs.MoveAxisRightRaw, 0, player.inputs.MoveAxisForwardRaw);
            cameraPlanarDirection = cameraContorller.CameraPlanarDirection;

            cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection);
            moveInputVectorOriented = cameraPlanarRotation * moveInputVector.normalized;

            //HandlePressed
            if (isAiming)
            {
                sprinting = player.inputs.Sprint.PressedDown() && (moveInputVector != Vector3.zero);
                isAiming = player.inputs.Aim.Pressed();
            }
            else
            {
                sprinting = player.inputs.Sprint.Pressed() && (moveInputVector != Vector3.zero);
                isAiming = player.inputs.Aim.PressedDown();

            }

            //MoveSpeed
            if (sprinting) targetSpeed = moveInputVector != Vector3.zero ? player.SprintSpeed : 0;
            else if (isAiming) targetSpeed = moveInputVector != Vector3.zero ? player.WalkSpeed : 0;
            else targetSpeed = moveInputVector != Vector3.zero ? player.RunSpeed : 0;
            newSpeed = Mathf.Lerp(newSpeed, targetSpeed, Time.deltaTime * moveSharpness);

            //Velocity
            bool isOnSlope = IsOnSlope();
            newVelocity = isOnSlope ? AdjustDirectionToSlope(moveInputVectorOriented) * newSpeed : moveInputVectorOriented * newSpeed;
            if (player.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") || (player.animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_A") ||
                player.animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_B")))
            {
                if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f)
                {
                    newVelocity = Vector3.zero;
                    aim.sprite = aimSprites[2];
                    aimRect.sizeDelta = new Vector2(75, 75);
                }
            }
        }

        private void CalculateRoataion()
        {
            //Rotation
            if (isAiming)
            {
                targetRotation = Quaternion.LookRotation(cameraPlanarDirection);
                newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness);
            }
            else if (targetSpeed != 0)
            {
                targetRotation = Quaternion.LookRotation(moveInputVectorOriented);
                newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness);
            }
        }

        public void OnMove()
        {
            //Debug.Log(playingAttack);
            transform.Translate(newVelocity * Time.deltaTime, Space.World); 
            transform.rotation = newRotation;

            //Animator
            if (isAiming)
            {
                strafeParameter = Mathf.Lerp(0, 1, strafeParameter+4*Time.deltaTime);
                strafeParameterXZ = Vector3.Lerp(strafeParameterXZ, moveInputVector * newSpeed, moveSharpness * Time.deltaTime);
                aim.sprite = aimSprites[1];
                float rectSize = Mathf.Lerp(aimRect.sizeDelta.x, 120, Time.deltaTime);
                aimRect.sizeDelta = Vector2.one* rectSize;
            }
            else
            {
                strafeParameter = Mathf.Lerp(0, 1, strafeParameter - 4 * Time.deltaTime);
                strafeParameterXZ = Vector3.Lerp(strafeParameterXZ, Vector3.forward * newSpeed, moveSharpness * Time.deltaTime);
                aim.sprite = aimSprites[0];
                aimRect.sizeDelta = new Vector2(50, 50);
            }

            player.animator.SetFloat("Strafing", strafeParameter);
            player.animator.SetFloat("StrafingX", Mathf.Round((strafeParameterXZ.x * 100f) / 100f));
            player.animator.SetFloat("StrafingZ", Mathf.Round((strafeParameterXZ.z * 100f) / 100f));
        }

        public void OnJump()
        {
            aim.sprite = aimSprites[0];
            aimRect.sizeDelta = new Vector2(50, 50);
            checkTime += Time.deltaTime;
            if (isGrounded)
                isFalling = false;
            else
                isFalling = true;

            if (isGrounded && player.inputs.Jump.PressedDown()&&checkTime>=jumpDelayTime)
            {
                isJumping = true;
                checkTime = 0f;
                player.rigidbody.AddForce(player.JumpForce * Vector3.up, ForceMode.Impulse);
            }
            else
                isJumping = false;

            player.animator.SetBool("IsJumping", isJumping);
            player.animator.SetBool("IsFalling", isFalling);
            player.animator.SetBool("IsGrounded", isGrounded);
        }


        private bool IsOnSlope()
        {
            Ray ray=new Ray(transform.position, Vector3.down);
            if(Physics.Raycast(ray,out slopeHit, RAY_DISTANCE, groundLayer))
            {
                var angle=Vector3.Angle(Vector3.up,slopeHit.normal);
                return angle != 0f &&angle<maxSlopeAngle;
            }
            return false;
        }

        private Vector3 AdjustDirectionToSlope(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
        }

        public void OnAttack()
        {
            if (skillATime >= skillACool && player.inputs.Attack.PressedDown() && (player.inputs.SkillA.Pressed() || player.inputs.SkillA.PressedDown()))
            {
                skillATime = 0f;
                isAttacking = false;
                skillA = true;
                skillB = false;
            }

            else if (skillBTime >= skillBCool && player.inputs.Attack.PressedDown() && (player.inputs.SkillB.Pressed() || player.inputs.SkillB.PressedDown()))
            {
                skillBTime = 0f;
                isAttacking = false;
                skillA = false;
                skillB = true;
                //player.rigidbody.AddForce(5f * Vector3.up, ForceMode.Impulse);
            }

            else if (attackTime >= attackCool && player.inputs.Attack.PressedDown())
            {
                attackTime = 0f;
                isAttacking = true;
                skillA = false;
                skillB = false;
            }
            else
            {
                isAttacking = false;
                skillA=false;
                skillB=false;
            }

            player.animator.SetBool("IsAttacking", isAttacking);
            player.animator.SetBool("SkillA", skillA);
            player.animator.SetBool("SkillB", skillB);
        }


        private void AttackCoolCalculate()
        {
            attackTime += Time.deltaTime;
            skillATime += Time.deltaTime;
            skillBTime += Time.deltaTime;

        }

        private void CheckAlive()
        {
            if (player.CurrentHP <= 0)
                isDieing = true;
            else
            {
                hpBar.fillAmount =Mathf.Lerp(hpBar.fillAmount,(float) player.CurrentHP / player.MaxHP,Time.deltaTime);
                hpRect.anchoredPosition = new Vector2((-1)*(125-hpBar.fillAmount * 250f), 0);
                isDieing = false;
            }
        }
        
        public void OnDamage(float damage)
        {
            player.OnUpdateStat(player.MaxHP, player.CurrentHP - damage, player.Attack, player.Armor);
            Debug.Log("공격받았다"+ player.CurrentHP);
        }

        public void OnDead()
        {
            Debug.Log("플레이어가 죽었다");
            player.animator.SetBool("IsDieing", isDieing);
        }

        void Update()
        {
            //Debug.Log(isGrounded);
            CaculateVelocity();
            CalculateRoataion();
            AttackCoolCalculate();
            CheckAlive();
        }
    }
}

