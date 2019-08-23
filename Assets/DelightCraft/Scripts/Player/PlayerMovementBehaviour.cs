using UnityEngine;

namespace DelightCraft.Core.Player
{
    /// <summary>
    /// プレイヤーの動作系
    /// 超しょぼい動きを提供しているので各自でよりFPSゲームらしい動きを作ってください。
    /// </summary>
    public class PlayerMovementBehaviour : MonoBehaviour
    {
        private Animator animator;
        private Vector3 velocity;

        [SerializeField] private float jumpPower = 5f;

        //　レイを飛ばす体の位置
        [SerializeField] private Transform charaRay;

        //　レイの距離
        [SerializeField] private float charaRayRange = 0.2f;

        //　レイが地面に到達しているかどうか
        private bool isGround;

        //　入力値
        private Vector3 input;

        //　歩く速さ
        [SerializeField] private float walkSpeed = 1.5f;

        //　rigidbody
        private Rigidbody rigid;
        private bool isGroundCollider = false;

        void Start()
        {
            animator = GetComponent<Animator>();
            velocity = Vector3.zero;
            isGround = false;
            rigid = GetComponent<Rigidbody>();
        }

        void Update()
        {
            //　キャラクターが接地していない時はレイを飛ばして確認
            if (!isGroundCollider)
            {
                if (Physics.Linecast(charaRay.position, (charaRay.position - transform.up * charaRayRange)))
                {
                    isGround = true;
                    rigid.useGravity = true;
                }
                else
                {
                    isGround = false;
                    rigid.useGravity = false;
                }

                Debug.DrawLine(charaRay.position, (charaRay.position - transform.up * charaRayRange), Color.red);
            }

            //　キャラクターコライダが接地、またはレイが地面に到達している場合
            if (isGroundCollider || isGround)
            {
                //　地面に接地してる時は初期化
                if (isGroundCollider)
                {
                    velocity = Vector3.zero;

                    //　着地していたらアニメーションパラメータと２段階ジャンプフラグをfalse
                    animator.SetBool("Jump", false);
                    rigid.useGravity = true;

                    //　レイを飛ばして接地確認の場合は重力だけは働かせておく、前後左右は初期化
                }
                else
                {
                    velocity = new Vector3(0f, velocity.y, 0f);
                }

                input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

                transform.position += Vector3.forward * (input.magnitude * walkSpeed);

                //　ジャンプ
                if (Input.GetButtonDown("Jump")
                    && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump")
                    && !animator.IsInTransition(0) //　遷移途中にジャンプさせない条件
                )
                {
                    animator.SetBool("Jump", true);
                    velocity.y += jumpPower;
                    rigid.useGravity = false;
                }
            }

            if (!isGroundCollider && !isGround)
            {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }
        }

        void FixedUpdate()
        {
            //　キャラクターを移動させる処理
            rigid.MovePosition(transform.position + velocity * Time.deltaTime);
        }

        void OnCollisionEnter()
        {
            Debug.DrawLine(charaRay.position, charaRay.position + Vector3.down, Color.blue);

            //　他のコライダと接触している時は下向きにレイを飛ばしFieldかBlockレイヤーの時だけ接地とする
            if (Physics.Linecast(charaRay.position, charaRay.position + Vector3.down,
                LayerMask.GetMask("Block")))
            {
                isGroundCollider = true;
            }
            else
            {
                isGroundCollider = false;
            }
        }

        //　接触していなければ空中に浮いている状態
        void OnCollisionExit()
        {
            isGroundCollider = false;
        }
    }
}