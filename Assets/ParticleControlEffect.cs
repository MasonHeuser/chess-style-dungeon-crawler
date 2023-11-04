using System.Collections;
using UnityEngine;

public class ParticleControlEffect : MonoBehaviour
{   
    //This variable acts as a sort of storage, its safe and secure but we don't reference this specifically to refer to it.
    private bool s_cleanUp = false;
    //Instead we reference this [abilityPoints] so that when it is referenced and recieved/changed an account can occur along with it.
    public bool cleanUp
    {
        get { return s_cleanUp; }
        set {
            s_cleanUp = value;
            if (s_cleanUp) {
                StartCoroutine(CleanUpGameObject());
            }
        }
    }
    ParticleSystem parSystem;
    ParticleSystem.Particle[] particles;

    void Start() {

        parSystem = GetComponent<ParticleSystem>();

        if (particles == null || particles.Length < parSystem.main.maxParticles)
            particles = new ParticleSystem.Particle[parSystem.main.maxParticles];
    }

    IEnumerator CleanUpGameObject() {

        parSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        while (true) {

            int numParticlesAlive = parSystem.GetParticles(particles);

            if (numParticlesAlive <= 0) {
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }
    }
}
