using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Core.Utilities;

namespace GrandmaGreen.Garden
{
    [CreateAssetMenu(menuName = "GrandmaGreen/Garden/GardenVFX")]
    public class GardenVFX : ScriptableObject
    {
        [System.Serializable]
        public class VFXPool
        {
            public ParticleSystem vfx;
            [HideInInspector] public List<ParticleSystem> particlePool;
            public Dictionary<Vector3, ParticleSystem> particleMap;

            public void PlayVFXAtSingle(Vector3 position, Vector3 offset, Quaternion orientation)
            {
                PlayVFXRoutine(position, offset, orientation).Start();
            }

            IEnumerator PlayVFXRoutine(Vector3 position, Vector3 offset, Quaternion orientation)
            {
                ParticleSystem particle = GrabFromPool();

                particle.transform.position = position + offset;
                particle.transform.rotation = orientation;

                particle.Play();
                while (particle?.isPlaying == true)
                {
                    yield return null;
                }

                if (particle != null)
                    ReturnToPool(particle);
            }

            public void PlayVFXAtLoop(Vector3 position, Vector3 offset, Quaternion orientation)
            {
                if (particleMap.ContainsKey(position))
                {
                    return;
                }

                ParticleSystem particle = GrabFromPool();

                particleMap.Add(position, particle);
                particleMap[position].transform.position = position + offset;
                particleMap[position].transform.rotation = orientation;

                particleMap[position].Play();
            }

            public void StopVFXAt(Vector3 position)
            {
                if (!particleMap.TryGetValue(position, out ParticleSystem particle)) return;

                particle.Stop();

                ReturnToPool(particle);
                particleMap.Remove(position);
            }

            public void Initalize()
            {
                particlePool = new List<ParticleSystem>();
                particleMap = new Dictionary<Vector3, ParticleSystem>();
            }

            public void Clear()
            {
                particlePool.Clear();
                particleMap.Clear();
            }

            ParticleSystem GrabFromPool()
            {
                ParticleSystem clone = null;
                if (particlePool.Count == 0)
                {
                    particlePool.Add(Instantiate(vfx));
                }

                clone = particlePool[0];
                particlePool.RemoveAt(0);

                return clone;
            }

            void ReturnToPool(ParticleSystem particleSystem)
            {
                particlePool.Add(particleSystem);
            }


        }

        public VFXPool GrowthParticleBurst;
        public VFXPool FertilizerParticleBurst;
        public VFXPool WateringParticleBurst;
        public VFXPool DryingUpBurst;

        static readonly Quaternion PARTICLE_ORIENTATION = Quaternion.Euler(-110, 0, 0);
        static readonly Vector3 PARTICLE_OFFSET = new Vector3(0, 0, -2);

        public void PlayGrowthParticle(Vector3 position) => PlayVFXSingle(GrowthParticleBurst, position, PARTICLE_OFFSET, PARTICLE_ORIENTATION);
        public void PlayWaterParticle(Vector3 position) => PlayVFXSingle(WateringParticleBurst, position, PARTICLE_OFFSET, PARTICLE_ORIENTATION);

        public void PlayFertilizerParticle(Vector3 position) => PlayVFXLooped(FertilizerParticleBurst, position, Vector3.zero, PARTICLE_ORIENTATION);
        public void StopFertilizerParticle(Vector3 position) => StopVFXLooped(FertilizerParticleBurst, position);


        public void PlayDryParticle(Vector3 position) => PlayVFXLooped(DryingUpBurst, position, PARTICLE_OFFSET, PARTICLE_ORIENTATION);
        public void StopDryParticle(Vector3 position) => StopVFXLooped(DryingUpBurst, position);

        public void Initalize()
        {
            GrowthParticleBurst.Initalize();
            FertilizerParticleBurst.Initalize();
            WateringParticleBurst.Initalize();
            DryingUpBurst.Initalize();
        }

        public void Clear()
        {
            GrowthParticleBurst.Clear();
            FertilizerParticleBurst.Clear();
            WateringParticleBurst.Clear();
            DryingUpBurst.Clear();
        }


        void PlayVFXSingle(VFXPool particleSystem, Vector3 position, Vector3 offset, Quaternion orientation)
        {
            particleSystem.PlayVFXAtSingle(position, offset, orientation);
        }

        void PlayVFXLooped(VFXPool particleSystem, Vector3 position, Vector3 offset, Quaternion orientation)
        {
            particleSystem.PlayVFXAtLoop(position, offset, orientation);
        }

        void StopVFXLooped(VFXPool particleSystem, Vector3 position)
        {
            particleSystem.StopVFXAt(position);
        }
    }
}
