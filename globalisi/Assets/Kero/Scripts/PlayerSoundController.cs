using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    // Gizli deðiþkenler (Inspector'da görünmesine gerek yok, kod kendi bulacak)
    private AudioSource footstepSource;
    private AudioSource sfxSource;

    [Header("Ses Dosyalarý")]
    public AudioClip[] footstepSounds; // Adým sesleri buraya
    public AudioClip switchViewSound;  // Maske sesi buraya

    [Header("Ayarlar")]
    public float stepRate = 0.5f;
    private float nextStepTime = 0f;

    void Awake()
    {
        // --- OTOMATÝK AYIRICI ---
        // Objenin üzerindeki tüm Audio Source'larý bul
        AudioSource[] sources = GetComponents<AudioSource>();

        // Eðer en az 2 tane varsa daðýt
        if (sources.Length >= 2)
        {
            footstepSource = sources[0]; // Birincisi Yürüme olsun
            sfxSource = sources[1];      // Ýkincisi Maske olsun
        }
        else
        {
            // Eðer eksik varsa kod kendi yaratsýn (Garanti olsun)
            footstepSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Hareket ediyor muyuz?
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isMoving)
        {
            if (Time.time >= nextStepTime)
            {
                PlayFootstep();
                nextStepTime = Time.time + stepRate;
            }
        }
        else
        {
            // Durunca sadece YÜRÜME hoparlörünü sustur.
            // Maske hoparlörü (sfxSource) farklý olduðu için etkilenmez.
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }

        // Maske Tak/Çýkar (V Tuþu)
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlaySwitchSound();
        }
    }

    public void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        int randIndex = Random.Range(0, footstepSounds.Length);
        footstepSource.pitch = Random.Range(0.9f, 1.1f);

        // Yürüme için PlayOneShot yerine Play kullanýyoruz ki Stop() diyince dunsun
        footstepSource.clip = footstepSounds[randIndex];
        footstepSource.Play();
    }

    public void PlaySwitchSound()
    {
        if (switchViewSound != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(switchViewSound);
        }
    }
}