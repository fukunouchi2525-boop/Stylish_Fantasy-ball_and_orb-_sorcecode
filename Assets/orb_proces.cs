using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class orb_proces : MonoBehaviour
{
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform sphere;
    private const int OrbCount = 10;
    [SerializeField] private float minimumSpacing = 1.25f;
    [SerializeField] private float collectDistance = 0.8f;
    [SerializeField] private float deathY = -5f;
    [SerializeField] private AudioClip bgm;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private Texture2D titleLogo;

    private readonly List<GameObject> orbs = new List<GameObject>();
    private BoxCollider stageCollider;
    private int score;
    private float elapsedTime;
    private bool gameOver;
    private bool cleared;
    private bool gameStarted;
    private AudioSource bgmSource;
    private AudioSource effectSource;

    void Awake()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip = bgm;
        bgmSource.loop = true;
        bgmSource.volume = 0.25f;

        effectSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        stageCollider = GetComponent<BoxCollider>();
        if (sphere == null)
        {
            GameObject sphereObject = GameObject.Find("Sphere");
            if (sphereObject != null) sphere = sphereObject.transform;
        }
        SetSpherePlayable(false);
    }

    void Update()
    {
        if (sphere == null || !gameStarted || gameOver) return;

        elapsedTime += Time.deltaTime;

        if (sphere.position.y < deathY)
        {
            EndGame(false);
            return;
        }

        for (int i = orbs.Count - 1; i >= 0; i--)
        {
            GameObject orb = orbs[i];
            if (orb == null)
            {
                orbs.RemoveAt(i);
                continue;
            }

            Vector3 difference = orb.transform.position - sphere.position;
            difference.y = 0f;
            if (difference.sqrMagnitude <= collectDistance * collectDistance)
            {
                Destroy(orb);
                orbs.RemoveAt(i);
                score++;
                if (collectSound != null) effectSource.PlayOneShot(collectSound);
            }
        }

        if (orbs.Count == 0)
        {
            EndGame(true);
        }
    }

    void SpawnOrbs()
    {
        if (orbPrefab == null || stageCollider == null) return;

        Bounds bounds = stageCollider.bounds;
        float edgePadding = minimumSpacing * 0.5f;
        List<Vector3> positions = new List<Vector3>();

        for (int orbIndex = 0; orbIndex < OrbCount; orbIndex++)
        {
            bool placed = false;
            for (int attempt = 0; attempt < 100; attempt++)
            {
                Vector3 position = new Vector3(
                    Random.Range(bounds.min.x + edgePadding, bounds.max.x - edgePadding),
                    bounds.max.y + 0.5f,
                    Random.Range(bounds.min.z + edgePadding, bounds.max.z - edgePadding)
                );

                if (IsFarEnough(position, positions))
                {
                    orbs.Add(Instantiate(orbPrefab, position, orbPrefab.transform.rotation));
                    positions.Add(position);
                    placed = true;
                    break;
                }
            }
            if (!placed) break;
        }
    }

    bool IsFarEnough(Vector3 candidate, List<Vector3> positions)
    {
        float minimumDistanceSquared = minimumSpacing * minimumSpacing;
        foreach (Vector3 position in positions)
        {
            Vector3 difference = candidate - position;
            difference.y = 0f;
            if (difference.sqrMagnitude < minimumDistanceSquared) return false;
        }
        return true;
    }

    void OnGUI()
    {
        if (!gameStarted)
        {
            DrawTitleScreen();
            return;
        }

        GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
        statusStyle.fontSize = 28;
        statusStyle.fontStyle = FontStyle.Bold;
        statusStyle.normal.textColor = Color.green;
        GUI.Label(new Rect(20, 20, 260, 42), "Score: " + score, statusStyle);
        GUI.Label(new Rect(20, 58, 260, 42), "Time: " + elapsedTime.ToString("F2", CultureInfo.InvariantCulture), statusStyle);

        if (gameOver)
        {
            Rect popup = new Rect(Screen.width * 0.1f, Screen.height * 0.15f, Screen.width * 0.8f, Screen.height * 0.7f);
            GUI.color = new Color(0f, 0f, 0f, 0.9f);
            GUI.Box(popup, GUIContent.none);
            GUI.color = Color.white;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontSize = 64;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = cleared ? Color.yellow : Color.red;
            GUI.Label(new Rect(popup.x, popup.y + 50, popup.width, popup.height * 0.35f), cleared ? "GAME CLEAR" : "GAME OVER", titleStyle);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 28;
            buttonStyle.fontStyle = FontStyle.Bold;
            if (GUI.Button(new Rect(popup.center.x - 170, popup.yMax - 120, 160, 60), "Restart", buttonStyle))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if (GUI.Button(new Rect(popup.center.x + 10, popup.yMax - 120, 160, 60), "Title", buttonStyle))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void DrawTitleScreen()
    {
        GUI.color = new Color(0f, 0f, 0f, 0.85f);
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);
        GUI.color = Color.white;

        if (titleLogo != null)
        {
            float width = Mathf.Min(Screen.width * 0.7f, 800f);
            float height = width * titleLogo.height / titleLogo.width;
            GUI.DrawTexture(new Rect((Screen.width - width) * 0.5f, Screen.height * 0.12f, width, height), titleLogo, ScaleMode.ScaleToFit, true);
        }

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 32;
        buttonStyle.fontStyle = FontStyle.Bold;
        float buttonWidth = Mathf.Min(300f, Screen.width * 0.6f);
        float buttonX = (Screen.width - buttonWidth) * 0.5f;

        if (GUI.Button(new Rect(buttonX, Screen.height * 0.76f, buttonWidth, 70f), "Play", buttonStyle))
        {
            BeginGame();
        }
    }

    void BeginGame()
    {
        gameStarted = true;
        SetSpherePlayable(true);
        SpawnOrbs();
        if (bgm != null) bgmSource.Play();
    }

    void SetSpherePlayable(bool playable)
    {
        if (sphere == null) return;

        sperecontrol controller = sphere.GetComponent<sperecontrol>();
        if (controller != null) controller.enabled = playable;

        Rigidbody body = sphere.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = !playable;
            body.useGravity = playable;
        }
    }

    void EndGame(bool isClear)
    {
        if (gameOver) return;
        gameOver = true;
        cleared = isClear;

        sperecontrol controller = sphere.GetComponent<sperecontrol>();
        if (controller != null) controller.enabled = false;

        Rigidbody body = sphere.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
        }

        if (bgmSource.isPlaying) bgmSource.Stop();
        if (gameOverSound != null)
        {
            effectSource.PlayOneShot(gameOverSound);
            StartCoroutine(RestartBgmAfterGameOver());
        }
        else if (bgm != null)
        {
            bgmSource.Play();
        }
    }

    IEnumerator RestartBgmAfterGameOver()
    {
        yield return new WaitForSeconds(gameOverSound.length);
        if (bgm != null) bgmSource.Play();
    }
}
