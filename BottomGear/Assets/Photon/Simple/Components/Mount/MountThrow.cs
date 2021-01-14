// ---------------------------------------------------------------------------------------------
// <copyright>PhotonNetwork Framework for Unity - Copyright (C) 2020 Exit Games GmbH</copyright>
// <author>developer@exitgames.com</author>
// ---------------------------------------------------------------------------------------------

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Photon.Pun.Simple
{
    [RequireComponent(typeof(Mount))]

    public class MountThrow : NetComponent
        , IOnPreUpdate
        , IOnPreSimulate
    {

        public KeyCode throwKey; // = KeyCode.Alpha5;
        public Mount mount;

        public bool fromRoot = true;
        public bool inheritRBVelocity = true;
        public Vector3 offset = new Vector3(0, 3f, 0);
        public Vector3 velocity = new Vector3(0, 1f, 5f);

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            if (mount == null)
                mount = GetComponent<Mount>();
        }
#endif
        public override void OnAwake()
        {
            base.OnAwake();
            if (mount == null)
                mount = GetComponent<Mount>();
        }


        public void OnPreUpdate()
        {
            if (!IsMine)
                return;

            if (Input.GetKeyDown(throwKey))
                throwQueued = true;
        }

        public bool throwQueued;
        public Vector3 deathVelocity = new Vector3();

        public void OnPreSimulate(int frameId, int subFrameId)
        {

            if (!throwQueued)
                return;

            throwQueued = false;


            var mountedObjs = mount.mountedObjs;

            for (int i = 0; i < mountedObjs.Count; ++i)
            {
                var obj = mountedObjs[i];

                // @carles -----
                Vector3 momentum = new Vector3();
                if (deathVelocity.magnitude == 0)
                {
                    var rb = transform.parent.GetComponent<Rigidbody>();
                    if (rb != null)
                        momentum = rb.velocity;
                }
                else
                {
                    momentum = deathVelocity;
                    deathVelocity = new Vector3(0, 0, 0);
                }
                    

                if (obj.IsThrowable)
                {
                    var syncState = obj as SyncState;

                    if (!syncState)
                        continue;

                    var origin = fromRoot ? mount.mountsLookup.transform : transform;

                    var localizedOffset = origin.TransformPoint(offset);
                    var localizedRotation = origin.rotation;
                    var localizedVelocity = (inheritRBVelocity) ? momentum + origin.TransformVector(velocity) : origin.TransformVector(velocity);

                    syncState.Throw(localizedOffset, localizedRotation, localizedVelocity);

                    continue;
                }
            }

        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MountThrow))]
    [CanEditMultipleObjects]
    public class MountThrowEditor : AccessoryHeaderEditor
    {


    }
#endif
}

