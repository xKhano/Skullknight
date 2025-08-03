using System.Collections;
using Cinemachine;
using Player.Statemachine;
using Skullknight.Core;
using Skullknight.Entity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Skullknight.Player.Statemachine
{
    public class PlayerController : EntityController<EPlayerState,PlayerController>
    {
        public static PlayerController Instance; //Singleton
        public Rigidbody2D rb;
        [FormerlySerializedAs("inputSystem")] public PlayerInput playerInput;
        [FormerlySerializedAs("attackImpulse")] [SerializeField] public CinemachineImpulseSource recoilImpulse;
        [SerializeField] public CinemachineImpulseSource bumpImpulse;
        [SerializeField] public AudioPlayer landingAudioPlayer;
        public float wallJumpForce = 2f;
        public float wallJumpDeathDuration = .25f;

        public BoxCollider2D ActiveBoxCollider2D 
        {
            get
            {
                return _activeBoxCollider2D;
            }
            set
            {
                if (_activeBoxCollider2D) _activeBoxCollider2D.enabled = false;
                _activeBoxCollider2D = value;
                if (_activeBoxCollider2D) _activeBoxCollider2D.enabled = true;
            }
        }
        public float Stamina
        {
            get
            {
                return m_stamina;
            }
            set
            {
                m_stamina = Mathf.Clamp(value, 0, m_maxStamina);
            }
        }
        public float MaxStamina => m_maxStamina;
        public float StaminaSpeed => m_staminaSpeed;
        public float SlidingSpeed => m_slidingSpeed;
        public float SlidingSpeedScale => m_SlidingSpeedScale;
        public float SlideStoppingSpeed => m_slideStoppingSpeed;
        public float FallingGravityScale => m_fallingGravityScale;
        public float SlidingSpeedCap => m_slidingSpeedCap;
        public float MaxFallingVelocity => m_maxFallingVelocity;
        public bool CanJump => (Time.time - lastJumpTime) > jumpCheckDeathSeconds;
    
        BoxCollider2D _activeBoxCollider2D;
        public BoxCollider2D standingCollider;
        public BoxCollider2D crouchCollider;
        public BoxCollider2D wallCheckCollider;
        public BoxCollider2D hangCheckCollider;
        public BoxCollider2D standingCheckCollider;
        public Collider2D hungObjectCollider;
        public BoxCollider2D atk0Collider;

        //Events
        [HideInInspector] public UnityEvent OnRoll;
        [HideInInspector] public UnityEvent OnLand;
        [HideInInspector] public UnityEvent OnCrouch;
        [HideInInspector] public UnityEvent OnSlide;
        [HideInInspector] public UnityEvent OnJump;
    
        //Member values
        public float rollingCooldown = 1f;
        public IHangPoint hangPoint;
        private float m_stamina = 100;
        private float m_maxStamina = 100;
        private float m_staminaSpeed = 20;
        private float m_groundSlideDeathSeconds = .5f;
        private float jumpCheckDeathSeconds = .1f;
        [SerializeField] private float m_fallingGravityScale = 3f;
        private float m_slidingSpeed = 2f;
        private float m_slidingSpeedCap = 7f;
        private float m_SlidingSpeedScale = .5f;
        private float m_slideStoppingSpeed = .2f;
        [SerializeField] private float m_jumpCutPercentage = .25f;
        [SerializeField] private float m_maxFallingVelocity = 6f;
        [SerializeField] private float m_maxAirstrafingVelocity = 2f;
        [SerializeField] private float m_airstrafingAcceleration = 1f;
        public float rollingVelocity = 10;
        public float maxRunningVelocity = 6.5f;
        public float maxCrouchingVelocity = 1f;
        public float maxRunningAnimSpeed = 2f;
        public float jumpVelocity = 5f;

        [Header("Transforms")]
        public Transform attackHitbox;

        [Header("Physic Materials")]
        public PhysicsMaterial2D slidingPhysicMaterial;
        public PhysicsMaterial2D normalPhysicMaterial;
        public PhysicsMaterial2D stoppingPhysicMaterial;
    

        [Header("CollisionCheckers")]
        public CollisionChecker groundCollisionChecker;
        public CollisionChecker standUpCollisionChecker;
    

        [Header("LayerMasks")]
        public LayerMask attackMask;
        public LayerMask landingMask;
        public LayerMask hangMask;
        public LayerMask wallMask;
    

        //COROUTINES
        public float lastJumpTime; 
        public Coroutine groundSlideDeathCoroutine;
        public Coroutine rollCoolDownCoroutine;
        public Coroutine wallSlideCheckDeathCoroutine;
        
        protected void Awake()
        {
            Instance = this;
            onHealthChanged = new UnityEvent<int>();
            health = 12;
            states.Add(EPlayerState.Idle, new PlayerIdleState(this));
            states.Add(EPlayerState.Running, new PlayerRunningState(this));
            states.Add(EPlayerState.Crouching, new PlayerCrouchingState(this));
            states.Add(EPlayerState.Roll, new PlayerRollState(this));
            states.Add(EPlayerState.Sliding, new PlayerSlidingState(this));
            states.Add(EPlayerState.Jumping, new PlayerJumpState(this));
            states.Add(EPlayerState.Falling, new PlayerFallingState(this));
            states.Add(EPlayerState.Hanging, new PlayerHangingState(this));
            states.Add(EPlayerState.Climbing, new PlayerClimbingState(this));
            states.Add(EPlayerState.Hurt, new PlayerHurtState(this));
            states.Add(EPlayerState.Wallsliding, new PlayerWallSlideState(this));
            states.Add(EPlayerState.CrouchAttack, new PlayerCrouchingAttackState(this));
            states.Add(EPlayerState.AttackOne, new PlayerAttackingState(this,"atk0",.33f,0.33f,EPlayerState.AttackTwo));
            states.Add(EPlayerState.AttackTwo, new PlayerAttackingState(this,"atk1",.33f,0.33f,null));
            states.Add(EPlayerState.Dead, new PlayerDeadState(this));
            
            ChangeState(EPlayerState.Idle);
            GameManager.Instance.OnStateChange.AddListener(OnGameStateChanged);
            SetDamageImmunity(false);
        }

        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.P))
            {
                ChangeState(EPlayerState.Hurt);
            }
        }

        public void SetDamageImmunity(bool isImmune)
        {
            isDamageable = !isImmune;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (currentState == null) ChangeState(EPlayerState.Idle);
        }

        void OnDisable()
        {
            if(playerInput != null) base.OnDisable();
            isDamageable = true;
            ChangeState(EPlayerState.Idle);
        }
        public void Roll()
        {
            m_stamina -= 50;
            rb.linearVelocity = Vector2.right * (playerInput.actions["Horizontal"].ReadValue<float>() * rollingVelocity);
            OnRoll?.Invoke();
        }
        public void MoveOnGround(float inputAxis)
        {
            float ACCELERATION = 1f;
            float diffToMaxVel = inputAxis * maxRunningVelocity - rb.linearVelocity.x;
            float fixedAcceleration = Mathf.Clamp(diffToMaxVel, -ACCELERATION, ACCELERATION);
            ActiveBoxCollider2D.sharedMaterial = normalPhysicMaterial;
            if (inputAxis > 0) SetFlip(false);
            else if (inputAxis < 0) SetFlip(true);
            rb.AddForce(Vector2.right * fixedAcceleration,ForceMode2D.Impulse);
        }
        public void Airstrafe(float inputAxis)
        {
            float diffToMaxVel = inputAxis * m_maxAirstrafingVelocity - rb.linearVelocity.x;
            float fixedAcceleration = Mathf.Clamp(diffToMaxVel, -m_airstrafingAcceleration, m_airstrafingAcceleration);
            ActiveBoxCollider2D.sharedMaterial = normalPhysicMaterial;
            if (inputAxis == 1f) SetFlip(false);
            else if (inputAxis == -1) SetFlip(true);
            rb.AddForce(Vector2.right * fixedAcceleration,ForceMode2D.Impulse);
        }

        public void Jump()
        {
            rb.AddForce(Vector2.up * jumpVelocity, ForceMode2D.Impulse);
            lastJumpTime = Time.time;
            OnJump?.Invoke();
        }
        public void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (CanJump)
            {
                ChangeState(EPlayerState.Jumping);
            }
        }
        public void OnHoldPerformed(InputAction.CallbackContext context)
        {
            IHangPoint point = GetHangPoint();
            if (point != null)
            {
                hangPoint = point;
                ChangeState(EPlayerState.Hanging);
            }
        }

        public void OnGameStateChanged(GameManager.EGameManagerState state)
        {
            switch (state)
            {
                case GameManager.EGameManagerState.Playing:
                    this.enabled = true;
                    break;
                case GameManager.EGameManagerState.Cutscene:
                    this.enabled = false;
                    break;
                case GameManager.EGameManagerState.EscapeMenu:
                    this.enabled = false;
                    break;
                case GameManager.EGameManagerState.Gameover:
                    this.enabled = false;
                    break;
            }
        }

        public void JumpCut()
        {
            rb.AddForce(Vector2.down * (rb.linearVelocity.y * m_jumpCutPercentage),ForceMode2D.Impulse);
        }

        public override bool TakeDamage(int amount)
        {
            bool result = false;
            if (isDamageable)
            {
                health = Mathf.Clamp(health - amount, 0, maxHealth);
                onHealthChanged?.Invoke(health);
                
                if (stateEnum == EPlayerState.Hanging)
                {
                    Unhang();
                    ChangeState(EPlayerState.Hurt);
                }
                else
                {
                    ChangeState(EPlayerState.Hurt);
                }

                return true;
            }

            return false;
        }
        public void SwordAttack()
        {
            var hits = Physics2D.BoxCastAll(atk0Collider.bounds.center, atk0Collider.bounds.size, 0, Vector3.up, .1f, attackMask);
            int hitCount = 0;
            foreach (var item in hits)
            {
                var damageablecomp = item.transform.GetComponent<IDamageable>();
                if (damageablecomp != null)
                {
                    hitCount++;
                    damageablecomp.TakeDamage(10);
                }
            }
            if(hitCount > 0 ) recoilImpulse.GenerateImpulse();
        }
        public void Crouchwalk(float inputAxis)
        {
            float ACCELERATION = .5f;
        
            float diffToMaxVel = inputAxis * maxCrouchingVelocity - rb.linearVelocity.x;
            float fixedAcceleration = Mathf.Clamp(diffToMaxVel, -ACCELERATION, ACCELERATION);
            ActiveBoxCollider2D.sharedMaterial = normalPhysicMaterial;
            rb.AddForce(Vector2.right * fixedAcceleration,ForceMode2D.Impulse);
        
            if (inputAxis > 0) SetFlip(false);
            else if (inputAxis < 0) SetFlip(true);
        }
        public IHangPoint GetHangPoint()
        {
            RaycastHit2D hitInfo;
            hitInfo = Physics2D.BoxCast(hangCheckCollider.bounds.center,
                hangCheckCollider.bounds.size,
                0f,
                rb.linearVelocity,
                .1f,
                hangMask);
            if (hitInfo.collider != null) return hitInfo.collider.GetComponent<IHangPoint>();
            else return null;
        }

        public void Hang()
        {
            SetFlip(hangPoint.SpriteFlip);
            rb.linearVelocity = Vector2.zero;
            rb.totalForce = Vector2.zero;
            rb.isKinematic = true;
            Physics2D.IgnoreCollision(standingCollider, hangPoint.HandCollider, true);
            Physics2D.IgnoreCollision(crouchCollider, hangPoint.HandCollider, true);
        
            transform.parent = hangPoint.HandCollider.transform;
            transform.localPosition = hangPoint.HoldPoint.localPosition;
        }

        public void Unhang()
        {
            transform.position = hangPoint.ClimbPoint.position;
            rb.MovePosition(hangPoint.ClimbPoint.position);
            rb.linearVelocity = Vector2.zero;
            transform.parent = null;
            rb.totalForce = Vector2.zero;
            rb.isKinematic = false;
            Physics2D.IgnoreCollision(standingCollider, hangPoint.HandCollider, false);
            Physics2D.IgnoreCollision(crouchCollider, hangPoint.HandCollider, false);
            hangPoint = null;
        }

        public void Groundslide()
        {
            rb.AddForce( rb.linearVelocity.x * SlidingSpeedScale * Vector2.right,ForceMode2D.Impulse);
            rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -SlidingSpeedCap,SlidingSpeedCap), rb.linearVelocity.y);
            if (groundSlideDeathCoroutine != null)
            {
                StopCoroutine(groundSlideDeathCoroutine);
                groundSlideDeathCoroutine = StartCoroutine(GroundslideDeath());
            }
        }

        public IEnumerator GroundslideDeath()
        {
            yield return new WaitForSeconds(m_groundSlideDeathSeconds);
            groundSlideDeathCoroutine = null;
        }
        public void SetFlip(bool value)
        {
            spriteRenderer.flipX = value;
            atk0Collider.transform.localScale = new Vector3(value ? -1 : 1,1,1);
        }

        public void RegenerateStamina()
        {
            Stamina += StaminaSpeed * Time.deltaTime;
        }
        public void ResetVelocity()
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}