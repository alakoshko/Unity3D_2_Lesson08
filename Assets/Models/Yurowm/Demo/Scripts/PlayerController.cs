using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Networking;

[RequireComponent (typeof (Animator))]
public class PlayerController : NetworkBehaviour {

	public Transform rightGunBone;
	public Transform leftGunBone;
	public Arsenal[] arsenal;
    private int _currentWeapon;

    private Animator animator;


    /// <summary>
    /// added from StandartAssets
    /// </summary>
    float m_TurnAmount;
    float m_ForwardAmount;
    Vector3 m_GroundNormal;
    bool m_IsGrounded;
    [SerializeField] float m_GroundCheckDistance = 0.1f;
    [SerializeField] float m_StationaryTurnSpeed = 180;
    [SerializeField] float m_MovingTurnSpeed = 360;
    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
    [SerializeField] float m_JumpPower = 12f;
    [SerializeField] float m_MoveSpeedMultiplier = 0.1f;
    //[SerializeField] float m_AnimSpeedMultiplier = 1f;

    Rigidbody m_Rigidbody;
    float m_OrigGroundCheckDistance;
    CapsuleCollider m_Capsule;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    bool m_Crouching;
    const float k_Half = 0.5f;
    float y;
    float z;

    Actions actions;

    
    void Start() {
        //Д.б. только локальный игрок
        if (!isLocalPlayer) return;

		animator = GetComponent<Animator> ();
		if (arsenal.Length > 0)
			SetArsenal (arsenal[0].name);

        actions = GetComponent<Actions>();

        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
    }



    public Arsenal ChangeArsenal()
    {
        //arsenal[_currentWeapon % arsenal.Length]. = false;
        _currentWeapon++;
        SetArsenal(arsenal[_currentWeapon % arsenal.Length].name);
        return arsenal[_currentWeapon % arsenal.Length];
    }

	public void SetArsenal(string name) {
		foreach (Arsenal hand in arsenal) {
            if (hand.name == name)
            {
                if (rightGunBone.childCount > 0)
                    Destroy(rightGunBone.GetChild(0).gameObject);
                if (leftGunBone.childCount > 0)
                    Destroy(leftGunBone.GetChild(0).gameObject);
                if (hand.rightGun != null)
                {
                    GameObject newRightGun = (GameObject)Instantiate(hand.rightGun);
                    newRightGun.transform.parent = rightGunBone;
                    newRightGun.transform.localPosition = Vector3.zero;
                    newRightGun.transform.localRotation = Quaternion.Euler(90, 0, 0);

                    //var newRightGun = (Transform)Instantiate(hand.rightGun, Vector3.zero, Quaternion.Euler(90, 0, 0)).transform;
                    //newRightGun.parent = rightGunBone;
                    //newRightGun.localPosition = Vector3.zero;
                    //newRightGun.localRotation = Quaternion.Euler(90, 0, 0);

                }
                if (hand.leftGun != null)
                {
                    GameObject newLeftGun = (GameObject)Instantiate(hand.leftGun);
                    newLeftGun.transform.parent = leftGunBone;
                    newLeftGun.transform.localPosition = Vector3.zero;
                    newLeftGun.transform.localRotation = Quaternion.Euler(90, 0, 0);
                }
                animator.runtimeAnimatorController = hand.controller;
                return;
            }
		}
	}

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            actions.SendMessage("Attack", SendMessageOptions.DontRequireReceiver);


        #region по простому 
        z = CrossPlatformInputManager.GetAxis("Vertical")  * m_StationaryTurnSpeed;
        y = CrossPlatformInputManager.GetAxis("Horizontal")  * m_StationaryTurnSpeed;
        //transform.Rotate(0, y, 0);
        if (Input.GetKey(KeyCode.S))
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0,transform.rotation.y,0), 0.2f);
        if (Input.GetKey(KeyCode.W))
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.rotation.y+180, 0), 0.2f);
        if (Input.GetKey(KeyCode.D))
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.rotation.y-90, 0), 0.2f);
        if (Input.GetKey(KeyCode.A))
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.rotation.y+90, 0), 0.2f);

        //z = CrossPlatformInputManager.GetAxis("Vertical") * Time.deltaTime * m_MoveSpeedMultiplier;
        //transform.Translate(0, 0, z);


        #endregion
    }
    
    private void FixedUpdate()
    {
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0 || Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                actions.SendMessage("Run", SendMessageOptions.DontRequireReceiver);
            else
                actions.SendMessage("Walk", SendMessageOptions.DontRequireReceiver);
        }
        else if(animator.GetFloat("Speed") > 0)
            actions.SendMessage("Stay", SendMessageOptions.DontRequireReceiver);
    }
    #region NavMeshTDA
    public void Move(Vector3 move, bool crouch, bool jump)
    {

        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (m_IsGrounded)
        {
            HandleGroundedMovement(crouch, jump);
        }
        else
        {
            HandleAirborneMovement();
        }

        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom();

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }
    void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }
    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);

        m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
    }
    void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!m_Crouching)
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
            }
        }
    }
    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (m_IsGrounded && crouch)
        {
            if (m_Crouching) return;
            m_Capsule.height = m_Capsule.height / 2f;
            m_Capsule.center = m_Capsule.center / 2f;
            m_Crouching = true;
        }
        else
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
                return;
            }
            m_Capsule.height = m_CapsuleHeight;
            m_Capsule.center = m_CapsuleCenter;
            m_Crouching = false;
        }
    }

    void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && !crouch && animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
        {
            // jump!
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;
            animator.applyRootMotion = false;
            m_GroundCheckDistance = 0.1f;
        }
    }
    public void OnAnimatorMove()
    {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (m_IsGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
    }

    void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        //animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        actions.SendMessage("Walk", SendMessageOptions.DontRequireReceiver);

        //animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
        //animator.SetBool("Crouch", m_Crouching);
        animator.SetBool("OnGround", m_IsGrounded);
        if (!m_IsGrounded)
        {
            //animator.SetFloat("Jump", m_Rigidbody.velocity.y);
            actions.SendMessage("Jump", SendMessageOptions.DontRequireReceiver);
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        //float runCycle =
        //    Mathf.Repeat(
        //        animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
        //float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
        //if (m_IsGrounded)
        //{
        //    animator.SetFloat("JumpLeg", jumpLeg);
        //}

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        //if (m_IsGrounded && move.magnitude > 0)
        //{
        //    animator.speed = m_AnimSpeedMultiplier;
        //}
        //else
        //{
        //    // don't use that while airborne
        //    animator.speed = 1;
        //}
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            animator.applyRootMotion = true;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
            animator.applyRootMotion = false;
        }
    }
    #endregion


    [System.Serializable]
	public struct Arsenal {
		public string name;
		public GameObject rightGun;
		public GameObject leftGun;
		public RuntimeAnimatorController controller;
	}
}
