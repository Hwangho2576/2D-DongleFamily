using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager gameManager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    bool isMerge;
    bool isAttach;


    public Rigidbody2D rigid;
    public CircleCollider2D circleColider;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;

    Vector3 mousePos;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circleColider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    private void OnDisable()
    {
        // 동글 변수 초기화
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        // 동글 트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        // 동글 물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circleColider.enabled = true;
    }

    private void Update()
    {
        if(isDrag)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // X축 경계 설정
            float leftBorder = -4.2f + transform.localScale.x / 2f;
            float rightBorder = 4.2f - transform.localScale.x / 2f;

            if (mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }

            mousePos.z = 0;
            mousePos.y = 8;
            //transform.position = Vector3.Lerp(transform.position, mousePos, 0.1f);
            transform.position = mousePos;
        }
    }

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        mousePos.y = 8;
        //transform.position = Vector3.Lerp(transform.position, mousePos, 0.1f);
        transform.position = mousePos;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isAttach)
            return;

        isAttach = true;
        gameManager.SfxPlay(GameManager.Sfx.Attach);

        //StartCoroutine(AttachRoutine());
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
            yield break;

        isAttach = true;
        gameManager.SfxPlay(GameManager.Sfx.Attach);
        yield return new WaitForSeconds(1f);

        isAttach = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if(level == other.level && !isMerge && !other.isMerge)
            {
                {
                    (float, float) mePos = (transform.position.x, transform.position.y);
                    (float, float) otherPos = (other.transform.position.x, other.transform.position.y);

                    // 내가 아래에 있을 때
                    // 동일한높이 이고, 내가 오른쪽에 있을 때
                    if (mePos.Item2 < otherPos.Item2 || (mePos.Item2 == otherPos.Item2 && mePos.Item1 > otherPos.Item1))
                    {
                        // 상대방 숨기기
                        other.Hide(transform.position);

                        // 레벨 7 미만일 때 나 레벨업
                        if (level < 7) LevelUp();

                        // 레벨 7 일 때 나 제거
                        else
                        {
                            this.Hide(transform.position); 
                            EffectPlay();
                        }
                    }
                }
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        rigid.simulated = false;
        circleColider.enabled = false;

        StartCoroutine(HideRoutine(targetPos));

    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            
        yield return null;
 
        gameManager.score += (int)Mathf.Pow(2, level);
        gameManager.maxScore = (int)Mathf.Max(gameManager.score, gameManager.maxScore);

        isMerge = false;
        gameObject.SetActive(false);
    }
    void LevelUp()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());

    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        gameManager.SfxPlay(GameManager.Sfx.LevelUp);
        EffectPlay();

        yield return new WaitForSeconds(0.3f);
        level++;

        gameManager.maxLevel = Mathf.Max(level, gameManager.maxLevel);
        isMerge = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime > 2)
            {
                // 빨간색으로 변경 (경고)
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }

            if(deadTime > 5)
            {
                // 게임 오버
                gameManager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }

    public void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;

        effect.Play();
    }
}
