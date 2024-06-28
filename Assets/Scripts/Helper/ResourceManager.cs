using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Singleton;
    private void Awake()
    {
        Singleton = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
    }

    public UI_TMatch TournamentMatchPrefab;
    public UI_TMatch TournamentMatchCompactPrefab;
}
