using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private GameObject walkSound;
    [SerializeField] private GameObject attackSound;
    [SerializeField] private GameObject blockSound;
    [SerializeField] private GameObject dashSound;
    [SerializeField] private GameObject jumpSound;
    [SerializeField] private GameObject deadSound;
    [SerializeField] private GameObject hurtSound;
    void Start()
    {

    }
    public void PlayFootStep()
    {
        walkSound.SetActive(true);
    }

    public void StopFootStep()
    {
        walkSound.SetActive(false);
    }

    public void PlaySound(SoundType type)
    {
        var sfx = GetSoundByType(type);
        StartCoroutine(PlaySoundProcess(type, sfx));
    }
    private IEnumerator PlaySoundProcess(SoundType type, GameObject sound)
    {
        sound.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        sound.SetActive(false);
    }

    private GameObject GetSoundByType(SoundType sound)
    {
        switch (sound)
        {
            case SoundType.Run:
                return walkSound;
            case SoundType.Attack:
                return attackSound;
            case SoundType.Block:
                return blockSound;
            case SoundType.Dash:
                return dashSound;
            case SoundType.Jump:
                return jumpSound;
            case SoundType.Dead:
                return deadSound;
            case SoundType.Hurt:
                return hurtSound;
            default:
                return walkSound;
        }
    }
}

public enum SoundType
{
    Run,
    Attack,
    Block,
    Dash,
    Jump,
    Dead,
    Hurt
}
