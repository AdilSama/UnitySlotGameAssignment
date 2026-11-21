using System.Collections;
using UnityEngine;

namespace SlotGame.Audio
{
    /// <summary>
    /// Centralized audio controller for UI clicks, reel spin loops, stops, and win/loss stingers.
    /// </summary>
    public class SlotAudioManager : MonoBehaviour
    {
        public static SlotAudioManager Instance;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource reelLoopSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip spinStartClip, reelSpinLoopClip, reelStopClip, winClip, loseClip;

        private void Awake() => Instance = this;

        public void PlayButtonClick() => PlayOneShot(buttonClickClip);

        public void PlaySpinStart()
        {
            PlayOneShot(spinStartClip);
            StartReelLoop();
        }

        public void StartReelLoop()
        {
            if (reelLoopSource == null || reelSpinLoopClip == null) 
                return;
            
            reelLoopSource.clip = reelSpinLoopClip;
            reelLoopSource.loop = true;
            
            if (!reelLoopSource.isPlaying)
                reelLoopSource.Play();
        }

        public void StopReelLoop()
        {
            if (reelLoopSource != null && reelLoopSource.isPlaying)
                reelLoopSource.Stop();
        }

        public void PlayReelStop() => PlayOneShot(reelStopClip);

        public void PlayWin()
        {
            StopReelLoop();
            StartCoroutine(PlayDelay(winClip));
        }

        public void PlayLose()
        {
            StopReelLoop();
            StartCoroutine(PlayDelay(loseClip));
        }

        IEnumerator PlayDelay(AudioClip clip)
        {
            yield return new WaitForSeconds(0.4f);
            PlayOneShot(clip);
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (sfxSource == null || clip == null) 
                return;
            
            sfxSource.PlayOneShot(clip);
        }
    }
}
