using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Funzilla
{
	internal class SoundManager : Singleton<SoundManager>
	{
		private AudioSource _sfxPlayer;
		private AudioSource _musicPlayer;
		private string _currentMusicKey = string.Empty;

		[SerializeField] private List<AudioClip> clips;

		private readonly LinkedList<AudioSource> _pitchedSfxPlayers = new LinkedList<AudioSource>();
		private readonly Dictionary<string, AudioClip> _dict = new Dictionary<string, AudioClip>();

		private AudioClip GetAudioClip(string key)
		{
			return _dict.TryGetValue(key, out var clip) ? clip : null;
		}

		internal static void PlaySfx(string key)
		{
			if (!Preference.SfxOn)
			{
				return;
			}
			var clip = Instance.GetAudioClip(key);
			if (clip)
			{
				Instance._sfxPlayer.PlayOneShot(clip);
			}
		}

		internal static void PlaySfx(string key, float pitch)
		{
			if (!Preference.SfxOn)
			{
				return;
			}
			var clip = Instance.GetAudioClip(key);
			if (clip == null)
			{
				return;
			}
			AudioSource player;
			if (Instance._pitchedSfxPlayers.First == null)
			{
				player = Instance.gameObject.AddComponent<AudioSource>();
			}
			else
			{
				player = Instance._pitchedSfxPlayers.First.Value;
				Instance._pitchedSfxPlayers.RemoveFirst();
			}
			player.pitch = pitch;
			player.PlayOneShot(clip);
			DOVirtual.DelayedCall(clip.length, () => { Instance._pitchedSfxPlayers.AddLast(player); });
		}

		internal static void PlayMusic(string key, bool loop = false, float delay = 0)
		{
			if (!Preference.MusicOn)
			{
				return;
			}

			if (Instance._currentMusicKey.Equals(key))
			{
				return;
			}

			var clip = Instance.GetAudioClip(key);
			if (!clip)
			{
				return;
			}
			Instance._currentMusicKey = key;

			// Play music logic here
			Instance._musicPlayer.Stop();
			Instance._musicPlayer.PlayOneShot(clip);
			Instance._musicPlayer.PlayDelayed(delay);
			Instance._musicPlayer.loop = loop;
		}

		private bool MusicPlaying => !string.IsNullOrEmpty(_currentMusicKey);

		internal static bool IsMusicPlaying(string music)
		{
			return Instance.MusicPlaying && music == Instance._currentMusicKey;
		}

		internal static void StopMusic()
		{
			Instance._currentMusicKey = string.Empty;
			Instance._musicPlayer.Stop();
		}

		internal static void ResumeMusic()
		{
			if (!Preference.MusicOn ||
				string.IsNullOrEmpty(Instance._currentMusicKey) ||
				Instance._musicPlayer.isPlaying)
			{
				return;
			}
			Instance._musicPlayer.Play();
		}

		internal static void SetLoopMusic(bool value)
		{
			Instance._musicPlayer.loop = value;
		}

		private void Awake()
		{
			var audioSources = GetComponents<AudioSource>();
			if (audioSources.Length < 2)
			{
				Array.Resize(ref audioSources, 2);
				for (var i = 0; i < 2; i++)
				{
					audioSources[i] = gameObject.AddComponent<AudioSource>();
				}
			}
			_sfxPlayer = audioSources[0];
			_musicPlayer = audioSources[1];

			if (clips == null) return;
			foreach (var clip in clips)
			{
				_dict.Add(clip.name, clip);
			}
		}
	}
}