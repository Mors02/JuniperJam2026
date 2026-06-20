using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    [System.Serializable]
    public class ExampleBombBarrage : ExampleAbility
    {
        [Header("Bomb Barrage Settings")]

        public Transform spawnTransform;
        public GameObject bombPrefab;
        public BlankMonobehavior abilityMonoHelper;

        public float orbitSpeed = 20f;
        public float attackDelay;

        public float spawnRadius = 5f;

        public float executionTime = 10f;
        public float executionRepeat = 0.5f;

        private float m_LastAttackTime;

        private float currentExecutionTime;
        private float currentExecutionRepeat;

        private Coroutine m_Coroutine;

        public override void Initialize(GameObject go = null)
        {
            base.Initialize(go);

            executionType = POAbilityExecutionType.ExternalControl;

            m_LastAttackTime = Time.time;

            DeactivateObjects();
        }

        public override void ExternalStart()
        {
            base.ExternalStart();

            if (referenceDict.ContainsKey(nameof(activeObjects)))
            {
                activeObjects = (List<GameObject>)referenceDict[nameof(activeObjects)];
            }

            if (!spawnTransform && referenceDict.TryGetValue("spawnTransform", out var spT))
            {
                spawnTransform = (Transform)spT;
            }

            if (!abilityMonoHelper && referenceDict.ContainsKey(nameof(abilityMonoHelper)))
                abilityMonoHelper = (BlankMonobehavior)referenceDict[nameof(abilityMonoHelper)];
            if (!bombPrefab && referenceDict.ContainsKey("bombPrefab"))
                bombPrefab = (GameObject)referenceDict["bombPrefab"];
            if (!target && referenceDict.ContainsKey("target"))
                target = (Transform)referenceDict["target"];

            DeactivateObjects();
        }

        protected override void OnDeepCopy()
        {
            base.OnDeepCopy();

            spawnTransform = null;
            activeObjects = new();
            abilityMonoHelper = null;
            bombPrefab = null;
            target = null;
        }

        public override bool CanTrigger()
        {
            return Time.time - m_LastAttackTime > attackDelay && activeObjects.Count > 0 &&
                spawnTransform && bombPrefab && abilityMonoHelper;
        }

        public override void Trigger()
        {
            base.Trigger();

            m_LastAttackTime = Time.time;

            if (abilityMonoHelper && m_Coroutine == null)
            {
                ArrangeActiveObjects();
                m_Coroutine = abilityMonoHelper.StartCoroutine(BombEnumerator());
            }
        }

        void ArrangeActiveObjects(bool orbit = false)
        {
            int n = activeObjects.Count;

            if (n > 0 && spawnTransform)
            {
                float angleFac = 360f / n;
                Vector3 position = spawnTransform.position;

                for (int i = 0; i < n; i++)
                {
                    float angle = angleFac + angleFac * i;
                    if (activeObjects[i])
                    {
                        if (orbit)
                        {
                            activeObjects[i].transform.RotateAround(position, spawnTransform.forward, orbitSpeed * Time.deltaTime);
                        }
                        else
                        {
                            activeObjects[i].transform.position =
                                PointOnCircle(position, spawnRadius, angle);
                        }

                    }
                }
            }
        }

        public static Vector3 PointOnCircle(Vector3 center, float radius, float angleDeg)
        {
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float x = Mathf.Cos(angleRad) * radius;
            float y = Mathf.Sin(angleRad) * radius;
            float z = 0;

            return center + new Vector3(x, y, z);
        }



        public static Vector3 OrbitPosition(Vector3 center, float radius, Vector3 fac)
        {
            // Convert angles to radians
            float horRad = Mathf.Deg2Rad; // horizontal rotation around Y
            float verRad = Mathf.Deg2Rad; // vertical tilt from horizontal
            float phaseRad = Mathf.Deg2Rad; // optional phase rotation

            // Spherical coordinates
            float x = center.x + radius * Mathf.Cos(verRad) * Mathf.Cos(horRad + phaseRad);
            float y = center.y + radius * Mathf.Sin(verRad);
            float z = center.z + radius * Mathf.Cos(verRad) * Mathf.Sin(horRad + phaseRad);

            return new Vector3(fac.x * x, fac.y * y, fac.z * z);
        }

        IEnumerator BombEnumerator()
        {
            float curExecuteTime = executionTime;

            float elapsedTime = 0f;
            while (elapsedTime < curExecuteTime)
            {
                ArrangeActiveObjects(true);

                if (Time.time - currentExecutionRepeat > executionRepeat)
                {
                    currentExecutionRepeat = Time.time;
                    if (spawnTransform)
                    {
                        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
                        Vector3 randomPosition = spawnTransform.position + randomOffset;

                        if (activeObjects.Count > 0)
                        {
                            int randIndex = Random.Range(0, activeObjects.Count);
                            if (activeObjects[randIndex])
                            {
                                randomPosition = activeObjects[randIndex].transform.position;
                            }
                        }

                        var bombObj = Object.Instantiate(bombPrefab, randomPosition, spawnTransform.rotation);
                        if (bombObj && bombObj.TryGetComponent<ExampleBomb>(out var bomb))
                        {
                            bomb.targetPos = target.position;
                            bomb.delayTime = 0f;
                            bomb.owner = gameObject;
                        }
                    }
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            DeactivateObjects();

            m_Coroutine = null;
        }

    }

}

