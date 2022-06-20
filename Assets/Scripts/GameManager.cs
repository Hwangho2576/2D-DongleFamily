using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("------------[ Core ]")]
    public int maxLevel;
    public int score;
    public int maxScore;
    bool isOver;
    bool isShake;

    [Header("------------[ Obj Pooling ]")]
    public GameObject donglePrefab;
    public Transform dongleGroup;
    List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    List<ParticleSystem> effectPool;
    Dongle lastDongle;

    [Header("------------[ Audio ]")]
    public AudioSource audioSourceBGM;
    public AudioSource[] audioSourceSFX;
    public AudioClip[] audioClipSFX;
    public enum Sfx { LevelUp, Next, Attach, Button, GameOver};
    int cursorSFX;

    [Header("------------[ UI ]")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject itemPanel;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;

    [Header("------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;

    private void Awake()
    {
        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();

        if (!PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);

        maxScore = PlayerPrefs.GetInt("MaxScore");
    }
    public void GameStart()
    {
        // ������Ʈ Ȱ��ȭ
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);

        // ��ŸƮ �г� ��Ȱ��ȭ
        startPanel.SetActive(false);

        // BGM �÷���
        audioSourceBGM.Play();
        SfxPlay(Sfx.Button);

        // ù ���� ����
        Invoke("NextDongle", 1f);

    }

    Dongle MakeDongle()
    {
        // ����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // ���� ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.gameManager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for(int i = 0; i <donglePool.Count; i++)
        {
            if (!donglePool[i].gameObject.activeSelf)
                return donglePool[i];
        }

        return MakeDongle();
    }

    void NextDongle()
    {
        if (isOver)
            return;

        lastDongle = GetDongle();

        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);
    }

    IEnumerator NextDongleRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        SfxPlay(Sfx.Next);
        NextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null || isShake)
            return;

        lastDongle.Drop();
        lastDongle = null;

        StartCoroutine(NextDongleRoutine());
    }

    public void GameOver()
    {
        if (isOver)
            return;

        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        // ���� ��������
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        // ���� ���� ȿ�� ��Ȱ��ȭ
        for(int i=0;i <dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false;
        }


        // ��� ���� ���ʷ� ����
        for (int i = 0; i < dongles.Length; i++)
        {
            SfxPlay(Sfx.LevelUp);
            dongles[i].EffectPlay();
            dongles[i].transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            dongles[i].gameObject.SetActive(false);

            yield return new WaitForSeconds(0.1f);

        }

        yield return new WaitForSeconds(1f);

        // �ְ� ���� ����
        PlayerPrefs.SetInt("MaxScore", maxScore);

        // ���� ���� UI ǥ��
        subScoreText.text = "���� : " + scoreText.text;
        gameOverPanel.SetActive(true);

        audioSourceBGM.Stop();
        SfxPlay(Sfx.GameOver);
    }

    public void Restart()
    {
        SfxPlay(Sfx.Button);

        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        SceneManager.LoadScene(0);
    }

    public void SfxPlay(Sfx type)
    {
        switch(type)
        {
            case Sfx.LevelUp:
                audioSourceSFX[cursorSFX].clip = audioClipSFX[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                audioSourceSFX[cursorSFX].clip = audioClipSFX[3];
                break;
            case Sfx.Attach:
                audioSourceSFX[cursorSFX].clip = audioClipSFX[4];
                break;
            case Sfx.Button:
                audioSourceSFX[cursorSFX].clip = audioClipSFX[5];
                break;
            case Sfx.GameOver:
                audioSourceSFX[cursorSFX].clip = audioClipSFX[6];
                break;
        }

        audioSourceSFX[cursorSFX].Play();
        cursorSFX = (cursorSFX + 1) % audioSourceSFX.Length;
    }

    public void ItemButton()
    {
        itemPanel.SetActive(true);
    }

    public void ShakeButton()
    {
        itemPanel.SetActive(false);

        Shake();
    }

    void Shake()
    {
        if (isShake) return;

        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        // ���� ��������
        isShake = true;

        Dongle[] dongles = FindObjectsOfType<Dongle>();

        for (int k = 0; k < 5; k++)
        {
            for (int i = 0; i < dongles.Length; i++)
            {
                dongles[i].rigid.AddForce(new Vector2(Random.Range(-1f,1f), 1) * 100);
            }

            yield return new WaitForSeconds(0.5f);
        }

        isShake = false;
    }

    private void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
        maxScoreText.text = maxScore.ToString();
    }
}
