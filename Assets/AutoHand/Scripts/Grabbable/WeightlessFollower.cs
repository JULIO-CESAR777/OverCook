using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    [DefaultExecutionOrder(999)]
    public class WeightlessFollower : MonoBehaviour {
        [HideInInspector]
        public Transform follow1 = null;
        [HideInInspector]
        public Transform follow2 = null;
        [HideInInspector]
        public Hand hand1 = null;
        [HideInInspector]
        public Hand hand2 = null;

        public Dictionary<Hand, Transform> heldMoveTo = new Dictionary<Hand, Transform>();

        [HideInInspector]
        public float followPositionStrength = 30;
        [HideInInspector]
        public float followRotationStrength = 30;

        [HideInInspector]
        public float maxVelocity = 5;

        [HideInInspector]
        public Grabbable grab;

        Transform _pivot = null;
        public Transform pivot {
            get {
                if(!gameObject.activeInHierarchy)
                    return null;

                if(_pivot == null) {
                    _pivot = new GameObject().transform;
                    _pivot.parent = transform.parent;
                    _pivot.name = "WEIGHTLESS PIVOT";
                }

                return _pivot;
            }
        }

        internal Rigidbody body;
        Transform moveTo = null;

        float startMass;
        float startDrag;
        float startAngleDrag;
        float startHandMass;
        float startHandDrag;
        float startHandAngleDrag;
        bool useGravity;


        public void Start() {
            if(body == null)
                body = GetComponent<Rigidbody>();

            if(startMass == 0) {
                startMass = body.mass;
                startDrag = body.linearDamping;
                startAngleDrag = body.angularDamping;
                useGravity = body.useGravity;
            }
        }





        public virtual void Set(Hand hand, Grabbable grab) {
            if (body == null)
                body = grab.body;

            if(moveTo == null) {
                moveTo = new GameObject().transform;
                moveTo.name = gameObject.name + " FOLLOW POINT";
                moveTo.parent = AutoHandExtensions.transformParent;
            }

            if(!heldMoveTo.ContainsKey(hand)) {
                heldMoveTo.Add(hand, new GameObject().transform);
                heldMoveTo[hand].name = "HELD FOLLOW POINT";
            }

            var tempTransform = AutoHandExtensions.transformRuler;
            tempTransform.position = hand.transform.position;
            tempTransform.rotation = hand.transform.rotation;

            var tempTransformChild = AutoHandExtensions.transformRulerChild;
            tempTransformChild.position = grab.transform.position;
            tempTransformChild.rotation = grab.transform.rotation;

            if(grab.maintainGrabOffset) {
                tempTransform.position = hand.moveTo.position + hand.grabPositionOffset;
                tempTransform.rotation = hand.moveTo.rotation * hand.grabRotationOffset;
            }
            else {
                tempTransform.position = hand.moveTo.position;
                tempTransform.rotation = hand.moveTo.rotation;
            }

            heldMoveTo[hand].parent = hand.moveTo;
            heldMoveTo[hand].position = tempTransformChild.position;
            heldMoveTo[hand].rotation = tempTransformChild.rotation;


            if(follow1 == null) {
                follow1 = heldMoveTo[hand];
                hand1 = hand;
            }
            else if(follow2 == null) {
                follow2 = heldMoveTo[hand];
                hand2 = hand;
                pivot.parent = body.transform;
                pivot.position = Vector3.Lerp(hand1.handGrabPoint.position, hand2.handGrabPoint.position, 0.5f);
                pivot.rotation = Quaternion.LookRotation((hand1.handGrabPoint.position - hand2.handGrabPoint.position).normalized, 
                                 Vector3.Lerp(hand1.handGrabPoint.up, hand2.handGrabPoint.up, 0.5f));
            }


            if (startMass == 0) {
                startMass = body.mass;
                startDrag = grab.preheldDrag;
                startAngleDrag = grab.preheldAngularDrag;
                useGravity = body.useGravity;
            }


            startHandMass = hand.body.mass;
            startHandDrag = hand.startDrag;
            startHandAngleDrag = hand.startAngularDrag;

            body.mass = startHandMass;
            body.linearDamping = startHandDrag;
            body.angularDamping = startHandAngleDrag;
            body.useGravity = false;

            followPositionStrength = hand.followPositionStrength;
            followRotationStrength = hand.followRotationStrength;


            maxVelocity = grab.maxHeldVelocity;
            this.grab = grab;

            hand.OnReleased += OnHandReleased;
        }


        void OnHandReleased(Hand hand, Grabbable grab){
            RemoveFollow(hand, heldMoveTo[hand]);
        }

        public virtual void FixedUpdate() {
            if(follow1 == null)
                return;
             
            MoveTo();
            TorqueTo();

            if(grab.HeldCount() == 0)
                Destroy(this);
        }



        protected void SetMoveTo() {
            if(follow1 == null || moveTo == null)
                return;

            if(follow2 != null) {
                moveTo.position = Vector3.Lerp(hand1.moveTo.position, hand2.moveTo.position, 0.5f);
                moveTo.rotation = Quaternion.LookRotation((hand1.moveTo.position - hand2.moveTo.position).normalized,
                                 Vector3.Lerp(hand1.moveTo.up, hand2.moveTo.up, 0.5f));
                moveTo.position -= pivot.position - pivot.parent.transform.position;
                moveTo.rotation *= Quaternion.Inverse(pivot.localRotation);
            }
            else {
                moveTo.position = follow1.position;
                moveTo.rotation = follow1.rotation;
            }
        }



        /// <summary>Moves the hand to the controller position using physics movement</summary>
        protected virtual void MoveTo() {
            if(followPositionStrength <= 0 || moveTo == null)
                return;

            SetMoveTo();


            var movePos = moveTo.position;
            var distance = Vector3.Distance(movePos, transform.position);

            distance = Mathf.Clamp(distance, 0, 0.5f);

            SetVelocity(0.55f);


            void SetVelocity(float minVelocityChange) {
                var velocityClamp = grab.maxHeldVelocity;

                Vector3 vel = (movePos - transform.position).normalized * followPositionStrength * distance;

                vel.x = Mathf.Clamp(vel.x, -velocityClamp, velocityClamp);
                vel.y = Mathf.Clamp(vel.y, -velocityClamp, velocityClamp);
                vel.z = Mathf.Clamp(vel.z, -velocityClamp, velocityClamp);

                body.linearVelocity = new Vector3(
                    Mathf.MoveTowards(body.linearVelocity.x, vel.x, minVelocityChange + Mathf.Abs(body.linearVelocity.x) * Time.fixedDeltaTime * 60),
                    Mathf.MoveTowards(body.linearVelocity.y, vel.y, minVelocityChange + Mathf.Abs(body.linearVelocity.y) * Time.fixedDeltaTime * 60),
                    Mathf.MoveTowards(body.linearVelocity.z, vel.z, minVelocityChange + Mathf.Abs(body.linearVelocity.z) * Time.fixedDeltaTime * 60)
                );
            }
        }


        /// <summary>Rotates the hand to the controller rotation using physics movement</summary>
        protected virtual void TorqueTo() {
            var moveRot = moveTo.rotation;
            var delta = (moveRot * Quaternion.Inverse(body.rotation));
            delta.ToAngleAxis(out float angle, out Vector3 axis);

            if(float.IsInfinity(axis.x))
                return;

            if(angle > 180f)
                angle -= 360f;

            var multiLinear = Mathf.Deg2Rad * angle * followRotationStrength;
            Vector3 angular = multiLinear * axis.normalized;
            angle = Mathf.Abs(angle);

            var angleStrengthOffset = 1;// Mathf.Lerp(1f, 2f, angle / 180f);
            body.angularDamping = Mathf.Lerp(startAngleDrag + 5, startAngleDrag, angle/4f);


            body.angularVelocity = new Vector3(
                Mathf.MoveTowards(body.angularVelocity.x, angular.x, Mathf.Clamp(Mathf.Sqrt(Mathf.Abs(body.angularVelocity.x))/180f, 1, 4) *followRotationStrength * Time.fixedDeltaTime * 10 * angleStrengthOffset),
                Mathf.MoveTowards(body.angularVelocity.y, angular.y, Mathf.Clamp(Mathf.Sqrt(Mathf.Abs(body.angularVelocity.y))/180f, 1, 4) *followRotationStrength * Time.fixedDeltaTime * 10 * angleStrengthOffset),
                Mathf.MoveTowards(body.angularVelocity.z, angular.z, Mathf.Clamp(Mathf.Sqrt(Mathf.Abs(body.angularVelocity.z))/180f, 1, 4) *followRotationStrength * Time.fixedDeltaTime * 10 * angleStrengthOffset)
            );
        }


        int CollisionCount() {
            return grab.CollisionCount();
        }

        public void RemoveFollow(Hand hand, Transform follow) {
            hand.OnReleased -= OnHandReleased;

            if(this.follow1 == follow) {
                this.follow1 = null;
                hand1 = null;
            }
            if(follow2 == follow) {
                follow2 = null;
                hand2 = null;
            }

            if(this.follow1 == null && follow2 != null) {
                this.follow1 = follow2;
                this.hand1 = hand2;
                hand2 = null;
                follow2 = null;
            }

            if(this.follow1 == null && follow2 == null && !grab.beingGrabbed) {
                if(body != null) {
                    body.mass = startMass;
                    body.linearDamping = startDrag;
                    body.angularDamping = startAngleDrag;
                    body.useGravity = useGravity;
                }
                Destroy(this);
            }

            heldMoveTo.Remove(hand);
        }

        private void OnDestroy()
        {
            if(moveTo != null)
                Destroy(moveTo.gameObject);

            foreach(var transform in heldMoveTo)
                Destroy(transform.Value.gameObject);

            if (body != null)
            {
                body.mass = startMass;
                body.linearDamping = startDrag;
                body.angularDamping = startAngleDrag;
                body.useGravity = useGravity;
            }
        }


    }


}
