using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Tracks")]
    [Tooltip("Nombre (sin extensión) de un AudioClip dentro de una carpeta Resources para usar como música de menú. Tiene PRIORIDAD. Vacíalo para usar el tema generado o la música del Inspector.")]
    [SerializeField] private string menuMusicResourceName = "berserk-guts";
    [Tooltip("Si no hay pista en Resources, usa el tema épico generado por código (EpicMenuMusic). Desmárcalo para usar 'mainMenuMusic'.")]
    [SerializeField] private bool useProceduralMenuMusic = true;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private List<AudioClip> inGameMusicLoop;
    private AudioClip _epicMenuClip; // cache del tema generado

    [Header("Global SFX")]
    [SerializeField] private List<AudioClip> enemyDeathSounds;
    [SerializeField] private AudioClip playerDeathSound;
    [SerializeField] private AudioClip startButtonSound;
    [SerializeField] private AudioClip genericButtonSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip openLevelUpSound;
    [SerializeField] private AudioClip closeLevelUpSound;
    [SerializeField] private AudioClip monsterAttackSound;
    [SerializeField] private AudioClip playerDamageSound;
    [SerializeField] private AudioClip swordSound;
[SerializeField] private AudioClip bowSound;
    [SerializeField] private AudioClip spearSound;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    public void SetMasterVolume(float vol)
    {
        masterVolume = vol;
        AudioListener.volume = vol;
    }

    public float GetMasterVolume() => masterVolume;

    public void SetMusicVolume(float vol)
    {
        musicVolume = vol;
        if (musicSource != null) musicSource.volume = vol;
    }

    public float GetMusicVolume() => musicVolume;

    public void SetSfxVolume(float vol)
    {
        sfxVolume = vol;
        if (sfxSource != null) sfxSource.volume = vol;
    }

    public float GetSfxVolume() => sfxVolume;

    private Coroutine musicLoopCoroutine;
    private bool isPlayerDead = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateMusicForCurrentScene();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        UpdateMusicForCurrentScene();
    }

    private void UpdateMusicForCurrentScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu")
        {
            PlayMainMenuMusic();
        }
        else if (sceneName == "Game")
        {
            StartInGameMusic();
        }
    }

    public void PlayMainMenuMusic()
    {
        isPlayerDead = false;
        StopMusic();

        AudioClip clip = null;

        // 1) Pista importada en una carpeta Resources (p.ej. berserk-guts) — tiene prioridad
        if (!string.IsNullOrEmpty(menuMusicResourceName))
            clip = Resources.Load<AudioClip>(menuMusicResourceName);

        // 2) Si no, el tema épico generado por código
        if (clip == null && useProceduralMenuMusic)
        {
            if (_epicMenuClip == null) _epicMenuClip = EpicMenuMusic.Generate();
            clip = _epicMenuClip;
        }

        // 3) Si no, la música asignada en el Inspector
        if (clip == null) clip = mainMenuMusic;

        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void StartInGameMusic()
    {
        isPlayerDead = false;
        StopMusic();
        if (inGameMusicLoop != null && inGameMusicLoop.Count > 0)
        {
            musicLoopCoroutine = StartCoroutine(InGameMusicRoutine());
        }
    }

    private IEnumerator InGameMusicRoutine()
    {
        int currentIndex = 0;
        while (!isPlayerDead)
        {
            AudioClip clip = inGameMusicLoop[currentIndex];
            if (clip == null) { currentIndex = (currentIndex + 1) % inGameMusicLoop.Count; yield return null; continue; }
            
            musicSource.clip = clip;
            musicSource.loop = false;
            musicSource.volume = musicVolume;
            musicSource.Play();

            yield return new WaitForSeconds(clip.length);

            currentIndex = (currentIndex + 1) % inGameMusicLoop.Count;
        }
    }

    public void StopMusic()
    {
        if (musicLoopCoroutine != null)
        {
            StopCoroutine(musicLoopCoroutine);
            musicLoopCoroutine = null;
        }
        musicSource.Stop();
    }

    public void HandlePlayerDeath()
    {
        isPlayerDead = true;
        StopMusic();
        PlaySFX(playerDeathSound);
    }

    public void PlayEnemyDeathSound()
    {
        if (enemyDeathSounds != null && enemyDeathSounds.Count > 0)
        {
            PlaySFX(enemyDeathSounds[Random.Range(0, enemyDeathSounds.Count)]);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlayStartButtonSound() => PlaySFX(startButtonSound);
    public void PlayGenericButtonSound() => PlaySFX(genericButtonSound);
    public void PlayPickupSound() => PlaySFX(pickupSound);
    public void PlayFootstepSound() => PlaySFX(footstepSound);
    public void PlayLevelUpSound() => PlaySFX(levelUpSound);
    public void PlayOpenLevelUpSound() => PlaySFX(openLevelUpSound);
    public void PlayCloseLevelUpSound() => PlaySFX(closeLevelUpSound);
    public void PlayMonsterAttackSound() => PlaySFX(monsterAttackSound);
    public void PlayPlayerDamageSound() => PlaySFX(playerDamageSound);
    public void PlaySwordSound() => PlaySFX(swordSound);
public void PlayBowSound() => PlaySFX(bowSound);
    public void PlaySpearSound() => PlaySFX(spearSound);
}
