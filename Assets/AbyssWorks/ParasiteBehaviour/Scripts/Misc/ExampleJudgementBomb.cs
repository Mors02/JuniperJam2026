using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    [System.Serializable]
    public class ExampleJudgementBomb : ExampleAbility
    {
        [Header("JudgementBomb")]

        public BlankMonobehavior abilityMonoHelper;
        public GameObject bombPrefab;
        public int noOfBombs = 20;
        public float spawnRadius = 10f;
        public float orbitSpeed = 200f;
        public float ariseSpeed = 2f;
        public float waitBeforeAriseTime = 2f;
        public float ariseDuration = 5f;
        public float waitBeforeFallTime = 2f;
        public Vector2 randomDropTimeRange = new Vector2(1f, 2f);
        public Vector3 spawnOffset = Vector3.up;

        private ExampleBomb[] bombs;
        private Vector3 targetPos;
        private Coroutine m_Coroutine;

        public override void Initialize(GameObject go = null)
        {
            base.Initialize(go);

            executionType = POAbilityExecutionType.ExternalControl;
        }

        public override void ExternalStart()
        {
            base.ExternalStart();

            if (!abilityMonoHelper && referenceDict.ContainsKey(nameof(abilityMonoHelper)))
                abilityMonoHelper = (BlankMonobehavior)referenceDict[nameof(abilityMonoHelper)];
            if (!bombPrefab && referenceDict.ContainsKey("bombPrefab"))
                bombPrefab = (GameObject)referenceDict["bombPrefab"];
            if (!target && referenceDict.ContainsKey("target"))
                target = (Transform)referenceDict["target"];
        }

        protected override void OnDeepCopy()
        {
            base.OnDeepCopy();

            abilityMonoHelper = null;
            bombPrefab = null;
            target = null;
        }

        public override bool CanTrigger()
        {
            return base.CanTrigger() && target && bombPrefab && abilityMonoHelper;
        }

        public override void Trigger()
        {
            base.Trigger();

            if (!abilityMonoHelper || m_Coroutine != null)
            {
                return;
            }

            bombs = new ExampleBomb[noOfBombs];
            float angleFac = 360f / noOfBombs;
            targetPos = target.position;

            for (int i = 0; i < noOfBombs; i++)
            {
                float angle = angleFac + angleFac * i;
                Vector3 position = PointOnCircle(target.position, spawnRadius, angle) + spawnOffset;

                var bombObj = Object.Instantiate(bombPrefab, position, Quaternion.identity);

                if (bombObj && bombObj.TryGetComponent<ExampleBomb>(out var bomb))
                {
                    float rand = Random.Range(randomDropTimeRange.x, randomDropTimeRange.y);
                    float totalTime = waitBeforeAriseTime + ariseDuration + waitBeforeFallTime + rand;
                    bomb.delayTime = totalTime;
                    bomb.targetPos = targetPos;
                    bomb.owner = gameObject;

                    bombs[i] = bomb;
                }
            }

            if (abilityMonoHelper) m_Coroutine = abilityMonoHelper.StartCoroutine(JudgementEnumerator());
        }

        public static Vector3 PointOnCircle(Vector3 center, float radius, float angleDeg)
        {
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float x = Mathf.Cos(angleRad) * radius;
            float y = 0;
            float z = Mathf.Sin(angleRad) * radius;

            return center + new Vector3(x, y, z);
        }

        IEnumerator JudgementEnumerator()
        {
            float ariseWaitDur = waitBeforeAriseTime;
            float ariseDur = ariseDuration;
            float fallWaitDur = waitBeforeFallTime;

            float elapsedTime = 0f;
            Vector3 orbitPos = targetPos;

            if (target)
            {
                targetPos = target.position;
                orbitPos = targetPos;
            }

            while (elapsedTime < ariseWaitDur)
            {
                if (target)
                {
                    targetPos = target.position;
                    //orbitPos = target.position;
                }

                foreach (var bomb in bombs)
                {
                    if (bomb)
                    {
                        bomb.transform.RotateAround(orbitPos, Vector3.up, orbitSpeed * Time.deltaTime);
                        bomb.targetPos = targetPos;
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            elapsedTime = 0f;
            while (elapsedTime < ariseDur)
            {
                if (target)
                {
                    targetPos = target.position;
                }

                foreach (var bomb in bombs)
                {
                    if (bomb)
                    {
                        bomb.transform.RotateAround(orbitPos, Vector3.up, orbitSpeed * Time.deltaTime);
                        bomb.transform.position += ariseSpeed * Time.deltaTime * Vector3.up;
                        bomb.targetPos = targetPos;
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            elapsedTime = 0f;
            while (elapsedTime < fallWaitDur)
            {
                if (target)
                {
                    targetPos = target.position;
                }

                foreach (var bomb in bombs)
                {
                    if (bomb)
                    {
                        bomb.transform.RotateAround(orbitPos, Vector3.up, orbitSpeed * Time.deltaTime);
                        bomb.targetPos = targetPos;
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            m_Coroutine = null;
        }
    }

}

