using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AbyssWorks.ParasiteBehaviour.Misc
{
    //[MovedFrom(true, null, null, "ChangedAbility")]
    [System.Serializable]
    public class ExampleQuickShot : ExampleAbility
    {
        private Transform spawnTransform;
        public GameObject bombPrefab;
        public string testScr = "Test";

        public override void Initialize(GameObject go = null)
        {
            base.Initialize(go);
        }

        public override void ExternalStart()
        {
            base.ExternalStart();


            if (!spawnTransform && referenceDict.TryGetValue("spawnTransform", out var spT))
            {
                spawnTransform = (Transform)spT;
            }

            if (!bombPrefab && referenceDict.ContainsKey("bombPrefab"))
                bombPrefab = (GameObject)referenceDict["bombPrefab"];
            if (!target && referenceDict.ContainsKey("target"))
                target = (Transform)referenceDict["target"];
        }

        protected override void OnDeepCopy()
        {
            base.OnDeepCopy();

            spawnTransform = null;
            bombPrefab = null;
            target = null;
        }

        public override bool CanTrigger()
        {
            return base.CanTrigger() && spawnTransform && bombPrefab && target;
        }

        public override void Trigger()
        {
            base.Trigger();

            if (bombPrefab && spawnTransform && target)
            {
                var bombObj = Object.Instantiate(bombPrefab, spawnTransform.position, spawnTransform.rotation);
                if (bombObj && bombObj.TryGetComponent<ExampleBomb>(out var bomb))
                {
                    bomb.targetPos = target.position;
                    bomb.delayTime = 0f;
                    bomb.owner = gameObject;
                }
            }
        }

        public override void ExternalUpdate()
        {
            base.ExternalUpdate();

            if (Input.GetKey(KeyCode.Space))
            {
                Debug.Log(testScr);
            }
        }
    }
}
