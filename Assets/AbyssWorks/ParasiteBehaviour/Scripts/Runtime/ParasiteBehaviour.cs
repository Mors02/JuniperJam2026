using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace AbyssWorks.ParasiteBehaviour
{
    [System.Serializable]
    public class ParasiteBehaviour : IEquatable<ParasiteBehaviour>
    {
        private Guid m_id = Guid.Empty;
        public Guid Id => m_id;

        public Dictionary<string, object> referenceDict = new();

        public GameObject gameObject { get; protected set; }

        public virtual void Initialize(GameObject go = null)
        {
            m_id = Guid.NewGuid();

            gameObject = go;
        }

        protected virtual void OnDeepCopy() { }

        public virtual void ExternalStart() { }
        public virtual void ExternalUpdate() { }
        public virtual void ExternalFixedUpdate() { }
        public virtual void ExternalExit() { }

        public virtual void ExternalCollisionEnter(Collision collision) { }
        public virtual void ExternalCollisionExit(Collision collision) { }
        public virtual void ExternalCollisionUpdate(Collision collision) { }
        public virtual void ExternalTriggerEnter(Collider other) { }
        public virtual void ExternalTriggerExit(Collider other) { }
        public virtual void ExternalTriggerUpdate(Collider other) { }

        /// <summary>
        /// Deep copies the parasite behaviour and retains type
        /// </summary>
        /// <param name="parasiteBehaviour"></param>
        /// <returns></returns>
        public static ParasiteBehaviour DeepCopyJson(ParasiteBehaviour parasiteBehaviour)
        {
            if (parasiteBehaviour == null) return null;
            string json = JsonUtility.ToJson(parasiteBehaviour);
            System.Type runtimeType = parasiteBehaviour.GetType();
            var pbCopy = (ParasiteBehaviour)JsonUtility.FromJson(json, runtimeType);
            pbCopy.OnDeepCopy();

            return pbCopy;
        }

        public bool Equals(ParasiteBehaviour other)
        {
            if (other is null) return false;

            return other.m_id == m_id;
        }

        public override bool Equals(object obj)
        {
            if (obj is null || obj is not ParasiteBehaviour) return false;
            return Equals((ParasiteBehaviour)obj);
        }

        public override int GetHashCode()
        {
            return m_id.GetHashCode();
        }


        public static bool operator ==(ParasiteBehaviour a, ParasiteBehaviour b) 
        {
            if (a is null && b is null) return true;
            return a is not null && a.Equals(b);
        }

        public static bool operator !=(ParasiteBehaviour a, ParasiteBehaviour b) => !(a == b);
    }
}

