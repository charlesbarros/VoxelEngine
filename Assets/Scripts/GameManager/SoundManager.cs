using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour 
{
	public bool _enableSFX = true;
	public int _sfxChannelsNumber = 5;
	public float _sfxVolume = 1.0f;
	public bool _enableFootStep = true;
	public float _footStepVolume = 1.0f;
	
	private AudioSource _footStepSource;
	private AudioSource[] _sfxChannel;
	private float[] _sfxChannelTimeStamp;
	
	private static SoundManager _instance;
	public static SoundManager Instance { get { return _instance; } }
	
	// Use this for initialization
	void Awake () 
	{
		_instance = this;
		
		_sfxChannel = new AudioSource[_sfxChannelsNumber];
		_sfxChannelTimeStamp = new float[_sfxChannelsNumber];
		
		for (int i=0; i<_sfxChannelsNumber; i++)
		{
			_sfxChannel[i] = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
			_sfxChannel[i].volume = _sfxVolume;
		}

		_footStepSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
		_footStepSource.loop = true;
		_footStepSource.volume = _footStepVolume;
		_footStepSource.playOnAwake = false;
	}
	
	#region FootStep
	public bool IsFootStepPlaying()
	{
		return _footStepSource.isPlaying;
	}
	
	public void PlayFootStep(AudioClip clip, float fadeTime = 0.0f)
	{
		if (_enableFootStep == true && clip != null)
		{
			_footStepSource.clip = clip;
			_footStepSource.Play();
		}
	}

	public void StopFootStep(float fadeTime = 0.0f)
	{
		if (_footStepSource.isPlaying)
		{
			_footStepSource.Stop();
		}
	}
	#endregion
	
	#region PlaySFX
	public AudioSource PlaySoundEffect(AudioClip clip, float volume = 1.0f)
	{
		if (_enableSFX && clip)
		{
			int channelIndex = 0;
			int leastImportantIndex = 0;
			
			// look for a free sound channel
			for (;channelIndex<_sfxChannelsNumber; channelIndex++)
			{
				if(!_sfxChannel[channelIndex].isPlaying)
				{
					_sfxChannel[channelIndex].Stop();
					_sfxChannel[channelIndex].clip = clip;
					_sfxChannel[channelIndex].loop = false;
					_sfxChannel[channelIndex].Play();
					_sfxChannel[channelIndex].volume = _sfxVolume * volume;
					_sfxChannelTimeStamp[channelIndex] = Time.time;
	
					return _sfxChannel[channelIndex];
				}

				if(_sfxChannel[leastImportantIndex].priority >= _sfxChannel[channelIndex].priority)
				{
					if(_sfxChannelTimeStamp[leastImportantIndex] > _sfxChannelTimeStamp[channelIndex])
					{
						leastImportantIndex = channelIndex;
					}
				}
			}
			// if could not find free channel
			if(channelIndex == _sfxChannelsNumber)
			{
				// stop the least important sound
				_sfxChannel[leastImportantIndex].Stop();
				_sfxChannel[leastImportantIndex].clip = clip;
				_sfxChannel[leastImportantIndex].loop = false;
				_sfxChannel[leastImportantIndex].Play();
				_sfxChannel[channelIndex].volume = _sfxVolume * volume;
				_sfxChannelTimeStamp[leastImportantIndex] = Time.time;
				return _sfxChannel[leastImportantIndex];
			}
		}
		
		return null;
	}
	#endregion
	
	#region StopSFX
	public void StopSFX(AudioClip audioToStop)
	{
		for (int channelIndex = 0;channelIndex<_sfxChannelsNumber; channelIndex++)
		{
			if(_sfxChannel[channelIndex].clip == audioToStop)
			{
				_sfxChannel[channelIndex].Stop();
			}
		}
	}
	
	public void StopAllSFX()
	{
		for (int channelIndex = 0;channelIndex<_sfxChannelsNumber; channelIndex++)
		{
			_sfxChannel[channelIndex].Stop();
			_sfxChannel[channelIndex].clip = null;
		}
	}
	#endregion
}
