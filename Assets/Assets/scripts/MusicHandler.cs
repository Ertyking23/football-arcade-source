using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MusicHandler : MonoBehaviour
{
    AudioSource source0;
    AudioSource source1;
    AudioSource currentAudioSource;

    [SerializeField] RectTransform nowPlayingText;
    [SerializeField] RectTransform songInfoBox;

    [SerializeField] Text songTitleText;
    [SerializeField] Text artistNameText;
    [SerializeField] Image albumArtImage;

    [SerializeField] AudioClip[] musicClips;
    [SerializeField] Song[] songs;

    [SerializeField] int musicVolume = 1;
    [SerializeField] int currentTrack = 0;

    // Start is called before the first frame update
    void Start()
    {

        source0 = gameObject.AddComponent<AudioSource>();
        source1 = gameObject.AddComponent<AudioSource>();

        source0.volume = musicVolume;
        source1.volume = 0;

        source0.loop = true;
        source1.loop = true;

        source0.playOnAwake = false;
        source0.playOnAwake = false;

        source0.clip = musicClips[0];

        currentAudioSource = source0;

        //InitSetList();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (musicClips.Length > 0)
            {

                NextSongRandom(1);
            }
        }

        ScrollNowPlayingText(50.0f);
        
    }

    void ScrollNowPlayingText(float speed)
    {
        nowPlayingText.anchoredPosition = new Vector3(nowPlayingText.anchoredPosition.x - (speed * Time.deltaTime), nowPlayingText.anchoredPosition.y, 0);
        if (nowPlayingText.anchoredPosition.x < -120)
        {
            nowPlayingText.anchoredPosition = new Vector3(320, nowPlayingText.anchoredPosition.y, 0);
        }
    }

    void InitSetList()
    {
        if (musicClips.Length > 0)
        {
            songs = new Song[musicClips.Length];
            for (int i = 0; i < musicClips.Length; i++)
            {
                Song iSong = new Song();
                iSong.clip = musicClips[i];

                //USE THIS ONLY IF YOU WANT TO SET UP THE SONG LIST IN CODE! OTHERWISE  USE THE INSPECTOR
                if (i == 0)
                {
                    iSong.artistName = "musicClips[0] artist name";
                    iSong.songName = "musicClips[0] song name";
                    iSong.albumArt = null;
                }
                if (i == 1)
                {
                    
                }
                if (i == 2)
                {
                    
                }
                if (i == 3)
                {
                    //ect..
                }

                songs[i] = iSong;
            }
        }
    }

    IEnumerator PopUpSongInfo(string artist_name, string song_title, Sprite album_art)
    {
        CancelInvoke("CloseSongInfoBox");
        StopCoroutine("CloseSongInfo");
        songInfoBox.anchoredPosition = new Vector3(songInfoBox.sizeDelta.x / 2, - songInfoBox.sizeDelta.y/2, 0);
        float infoBoxYPosition = songInfoBox.anchoredPosition.y;
        float targetInfoBoxYPosition = songInfoBox.sizeDelta.y / 2;

        artistNameText.text = artist_name;
        songTitleText.text = song_title;
        albumArtImage.sprite = album_art;
        while (infoBoxYPosition != targetInfoBoxYPosition)
        {
            infoBoxYPosition = Mathf.Lerp(infoBoxYPosition, targetInfoBoxYPosition, 5*Time.deltaTime);
            songInfoBox.anchoredPosition = new Vector3(songInfoBox.anchoredPosition.x, infoBoxYPosition, 0);
            float positionDifference = targetInfoBoxYPosition - infoBoxYPosition;
            if (positionDifference < .1f)
            {
                break;
            }
            yield return null;
        }
        Invoke("CloseSongInfoBox", 5.0f);
    }

    void CloseSongInfoBox()
    {
        StartCoroutine("CloseSongInfo");
    }

    IEnumerator CloseSongInfo()
    {
        float infoBoxYPosition = songInfoBox.anchoredPosition.y;
        float targetInfoBoxYPosition = -songInfoBox.sizeDelta.y / 2;
        while (infoBoxYPosition != targetInfoBoxYPosition)
        {
            infoBoxYPosition = Mathf.Lerp(infoBoxYPosition, targetInfoBoxYPosition, 5 * Time.deltaTime);
            songInfoBox.anchoredPosition = new Vector3(songInfoBox.anchoredPosition.x, infoBoxYPosition, 0);
            float positionDifference = infoBoxYPosition - targetInfoBoxYPosition;
            if (positionDifference < .1f)
            {
                break;
            }
            yield return null;
        }
    }

    public void StopLevelMusic()
    {
        StartCoroutine(FadeOutMusic(.5f));
    }

    public void NextSongRandom(float fadeDuration = .5f)
    {
        StartCoroutine("CrossFadeToRandomSong", fadeDuration);
        StartCoroutine(PopUpSongInfo(songs[currentTrack].artistName, songs[currentTrack].songName, songs[currentTrack].albumArt));
    }

    public IEnumerator CrossFadeToRandomSong(float fadeDuration = 0)
    {
        int i = currentTrack;
        while (i == currentTrack)
        {
            i = UnityEngine.Random.Range(0, songs.Length);
        }
        currentTrack = i;

        StartCoroutine(SwitchTrack(currentTrack, fadeDuration));
        yield return null;
    }

    public IEnumerator CrossFadeToNextSong(float fadeDuration = 0)
    {
        if (currentTrack == -1)
        {
            currentTrack = 0;
        }
        else
        {
            currentTrack++;
        }
        StartCoroutine(SwitchTrack(currentTrack, fadeDuration));
        yield return null;
    }

    IEnumerator SwitchTrack(int i, float fadeDuration = 0)
    {
        StopCoroutine("ScrollSongInfo");
        string[] clipPerams = new string[2];
        clipPerams[0] = songs[i].artistName;
        clipPerams[1] = songs[i].songName;
        StartCoroutine("ScrollSongInfo", clipPerams);
        bool play0 = true;
        if (source1.volume == 0)
            play0 = false;

        if (play0)
        {
            source0.clip = songs[i].clip;
            yield return StartCoroutine(CrossFade(source1, source0, fadeDuration));
        }
        else
        {
            source1.clip = songs[i].clip;
            yield return StartCoroutine(CrossFade(source0, source1, fadeDuration));
        }

        currentTrack = i;
        
        //CycleNextSong();
    }

    void CycleNextSong()
    {
        StartCoroutine("CycleSongs");
    }

    public void CancelCyclingSongs()
    {
        StopCoroutine("CycleSongs");
    }
    IEnumerator CycleSongs()
    {
        yield return new WaitForSeconds(songs[currentTrack].clip.length * .9f);
        NextSongRandom();
    }


    IEnumerator CrossFade(AudioSource a, AudioSource b, float duration)
    {
        float stepInterval = duration / 20f;
        float volumeInterval = musicVolume / 20f;

        b.Play();

        for (int i = 0; i < 20; i++)
        {
            a.volume -= volumeInterval;
            b.volume += volumeInterval;

            yield return new WaitForSeconds(stepInterval);
        }
        a.Stop();
    }

    IEnumerator FadeOutMusic(float duration = 0)
    {
        float stepInterval = duration / 20f;
        float volumeInterval = musicVolume / 20f;


        for (int i = 0; i < 20; i++)
        {
            source0.volume -= volumeInterval;
            source1.volume -= volumeInterval;

            yield return new WaitForSeconds(stepInterval);
        }

        source0.Stop();
        source1.Stop();

        StopAllCoroutines();
    }

    [Serializable]
    public struct Song
    {
        public AudioClip clip;
        public string artistName;
        public string songName;
        public Sprite albumArt;
    }
}
