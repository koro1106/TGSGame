using UnityEngine;
using UnityEngine.Playables;

public class SceneStartTimeline : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector director;

    void Start()
    {
        director.Play();
    }
}