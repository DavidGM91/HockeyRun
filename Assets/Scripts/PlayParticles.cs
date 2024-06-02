using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayParticles : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem ps;
    // Start is called before the first frame update
    void Start()
    {
        if (ps != null)
        {
            ps = GetComponent<ParticleSystem>();
            if(ps == null)
            {
                ps = GetComponentInChildren<ParticleSystem>();
            }
        }
    }
    public void play()
    {
        ps.Play();
    }
}
