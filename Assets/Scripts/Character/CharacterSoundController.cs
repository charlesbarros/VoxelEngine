using UnityEngine;
using System.Collections;

public class CharacterSoundController : MonoBehaviour 
{
	public AudioClip _walk;
	public AudioClip _run;
	public AudioClip _jump;
	public AudioClip _createBlock;
	public AudioClip _destroyBlock;
	public AudioClip _waterSplash;
	
	public void PlayWalk(bool run = false)
	{
		if (SoundManager.Instance.IsFootStepPlaying() == false)
		{
			if (run == true)
			{
				SoundManager.Instance.PlayFootStep(_run);
			}
			else
			{
				SoundManager.Instance.PlayFootStep(_walk);
			}
		}
	}
	
	public void StopWalk()
	{
		SoundManager.Instance.StopFootStep();
	}

	public void PlayJump()
	{
		SoundManager.Instance.PlaySoundEffect(_jump);
	}
	
	public void PlayCreateBlock()
	{
		SoundManager.Instance.PlaySoundEffect(_createBlock, 0.75f);
	}
	
	public void PlayDestroyBlock()
	{
		SoundManager.Instance.PlaySoundEffect(_destroyBlock, 3.5f);
	}
	
	public void PlayWaterSplash()
	{
		SoundManager.Instance.PlaySoundEffect(_waterSplash);
	}
}
