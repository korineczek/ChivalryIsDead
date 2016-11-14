﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CnControls
{
    [Flags]
    public enum ControlActionDirection
    {
        Horizontal = 0x1,
        Vertical = 0x2,
        Both = Horizontal | Vertical
    }

    /// <summary>
    /// Simple joystick class
    /// Contains logic for creating a simple joystick
    /// </summary>
    public class SimpleJoystickAction1 : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {

        /// <summary>
        /// Current event camera reference. Needed for the sake of Unity Remote input
        /// </summary>
        public Camera CurrentEventCamera { get; set; }

        // ------- Inspector visible variables ---------------------------------------

        /// <summary>
        /// The range in non-scaled pixels for which we can drag the joystick around
        /// </summary>
        public float MovementRange = 50f;

        /// <summary>
        /// The name of the horizontal axis for this joystick to update
        /// </summary>
        public string HorizontalAxisName = "Horizontal";

        /// <summary>
        /// The name of the vertical axis for this joystick to update
        /// </summary>
        public string VerticalAxisName = "Vertical";

        /// <summary>
        /// Should the joystick be hidden when the user releases the finger?
        /// [Space(15f)] attribute is needed only for the editor, it creates some spacing in the inspector
        /// </summary>
        [Space(15f)]
        [Tooltip("Should the joystick be hidden on release?")]
        public bool HideOnRelease;

        /// <summary>
        /// Should the joystick be moved along with the finger
        /// </summary>
        [Tooltip("Should the Base image move along with the finger without any constraints?")]
        public bool MoveBase = true;

        /// <summary>
        /// Should the joystick be moved along with the finger
        /// </summary>
        [Tooltip("Should the joystick snap to finger? If it's FALSE, the MoveBase checkbox logic will be ommited")]
        public bool SnapsToFinger = true;

        /// <summary>
        /// Joystick movement direction
        /// Specifies the axis along which it can move
        /// </summary>
        [Tooltip("Constraints on the joystick movement axis")]
        public ControlActionDirection JoystickMoveAxis = ControlActionDirection.Both;

        public GameObject ActionUI;

        /// <summary>
        /// Image of the joystick base
        /// </summary>
        [Tooltip("Image of the joystick base")]
        public Image ActionTop;

        /// <summary>
        /// Image of the joystick base
        /// </summary>
        [Tooltip("Image of the joystick base")]
        public Image ActionBottom;

        /// <summary>
        /// Image of the joystick base
        /// </summary>
        [Tooltip("Image of the joystick base")]
        public Image ActionLeft;

        /// <summary>
        /// Image of the joystick base
        /// </summary>
        [Tooltip("Image of the joystick base")]
        public Image ActionRight;

        /// <summary>
        /// Image of the joystick base
        /// </summary>
        [Tooltip("Image of the joystick base")]
        public Image ActionCenter;

        /// <summary>
        /// Image of the joystick base
        /// </summary>
        [Tooltip("Image of the joystick base")]
        public Image ActionCenterPushed;

        /// <summary>
        /// Image of the stick itself
        /// </summary>
        [Tooltip("Image of the stick itself")]
        public Image Stick;

        /// <summary>
        /// Rect Transform of the touch zone
        /// </summary>
        //[Tooltip("Touch Zone transform")]
        //public RectTransform TouchZone;

        //TESTING VARIABLES
        //TESTING VARIABLES
        //TESTING VARIABLES
        [Header("TESTING VARIABLES")]
        public MonsterAI monster1;
        public MonsterAI monster2;
        //TESTING VARIABLES
        //TESTING VARIABLES
        //TESTING VARIABLES

        // ---------------------------------------------------------------------------

        private Vector2 _initialStickPosition;
        private Vector2 _intermediateStickPosition;
        private Vector2 _initialBasePosition;
        //private Vector2 _initialBasePositionTop;
        //private Vector2 _initialBasePositionBottom;
        private RectTransform _baseTransform;
        //private RectTransform _baseTransformTop;
        //private RectTransform _baseTransformBottom;
        private RectTransform _stickTransform;

        private float _oneOverMovementRange;

        protected VirtualAxis HorizintalAxis;
        protected VirtualAxis VerticalAxis;

        public Text t;

        private float SX;
        private float SY;
        private bool moving;

        private float Rotation = 0;

        private bool attack;
        private bool taunt;
        private bool overreact;
        private bool cancel;
        private bool question;


        GameObject player;


        private PlayerActionController playerActionController;
        //Player playerScript;
        PlayerScript playerScript;

        private void Awake()
        {
            

            player = GameObject.FindGameObjectWithTag("Player").gameObject;
            //playerScript = player.GetComponent<Player>();
            playerScript = player.GetComponent<PlayerScript>();

            moving = false;
            SX = 0;
            SY = 0;

            _stickTransform = Stick.GetComponent<RectTransform>();
            _baseTransform = ActionUI.GetComponent<RectTransform>();
            //_baseTransformTop = ActionTop.GetComponent<RectTransform>();
            //_baseTransformBottom = ActionBottom.GetComponent<RectTransform>();

            _initialStickPosition = _stickTransform.anchoredPosition;
            _intermediateStickPosition = _initialStickPosition;
            //_initialBasePositionTop = _baseTransform.anchoredPosition;

            _stickTransform.anchoredPosition = _initialStickPosition;
            _baseTransform.anchoredPosition = _initialBasePosition;

            _oneOverMovementRange = 1f / MovementRange;

            if (HideOnRelease)
            {
                Hide(true);
            }

            playerActionController = player.GetComponent<PlayerActionController>();
        }


        void LateUpdate() {
            //if (moving) {
            //    playerScript.move(SX, SY);
            //}
        }

        private void OnEnable()
        {
            // When we enable, we get our virtual axis

            HorizintalAxis = HorizintalAxis ?? new VirtualAxis(HorizontalAxisName);
            VerticalAxis = VerticalAxis ?? new VirtualAxis(VerticalAxisName);

            // And register them in our input system
            CnInputManager.RegisterVirtualAxis(HorizintalAxis);
            CnInputManager.RegisterVirtualAxis(VerticalAxis);
        }

        private void OnDisable()
        {
            // When we disable, we just unregister our axis
            // It also happens before the game object is Destroyed
            CnInputManager.UnregisterVirtualAxis(HorizintalAxis);
            CnInputManager.UnregisterVirtualAxis(VerticalAxis);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            // Unity remote multitouch related thing
            // When we feed fake PointerEventData we can't really provide a camera, 
            // it has a lot of private setters via not created objects, so even the Reflection magic won't help a lot here
            // Instead, we just provide an actual event camera as a public property so we can easily set it in the Input Helper class
            CurrentEventCamera = eventData.pressEventCamera ?? CurrentEventCamera;

            // We get the local position of the joystick
            Vector3 worldJoystickPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_stickTransform, eventData.position,
                CurrentEventCamera, out worldJoystickPosition);

            // Then we change it's actual position so it snaps to the user's finger
            _stickTransform.position = worldJoystickPosition;
            // We then query it's anchored position. It's calculated internally and quite tricky to do from scratch here in C#
            var stickAnchoredPosition = _stickTransform.anchoredPosition;
            // Some bitwise logic for constraining the joystick along one of the axis
            // If the "Both" option was selected, non of these two checks will yield "true"
            if ((JoystickMoveAxis & ControlActionDirection.Horizontal) == 0)
            {
                stickAnchoredPosition.x = _intermediateStickPosition.x;
            }
            if ((JoystickMoveAxis & ControlActionDirection.Vertical) == 0)
            {
                stickAnchoredPosition.y = _intermediateStickPosition.y;
            }


            _stickTransform.anchoredPosition = stickAnchoredPosition;

            // Find current difference between the previous central point of the joystick and it's current position
            Vector2 difference = new Vector2(stickAnchoredPosition.x, stickAnchoredPosition.y) - _intermediateStickPosition;

            // Normalisation stuff
            var diffMagnitude = difference.magnitude;
            var normalizedDifference = difference / diffMagnitude;

            // If the joystick is being dragged outside of it's range
            if (diffMagnitude > MovementRange)
            {
                if (MoveBase && SnapsToFinger)
                {
                    // We move the base so it maps the new joystick center position
                    var baseMovementDifference = difference.magnitude - MovementRange;
                    var addition = normalizedDifference * baseMovementDifference;
                    _baseTransform.anchoredPosition += addition;
                    _intermediateStickPosition += addition;
                }
                else
                {
                    _stickTransform.anchoredPosition = _intermediateStickPosition + normalizedDifference * MovementRange;
                }
            }

            // We don't need any values that are greater than 1 or less than -1
            var horizontalValue = Mathf.Clamp(difference.x * _oneOverMovementRange, -1f, 1f);
            var verticalValue = Mathf.Clamp(difference.y * _oneOverMovementRange, -1f, 1f);

            //Debug.Log("HV " + horizontalValue);
            //Debug.Log("stickAnchoredPosition.x " + stickAnchoredPosition.x);
            //Debug.Log("Diff.x " + difference.x);
            //Debug.Log("_intermediateStickPosition " + difference.x);


            SX = Stick.rectTransform.localPosition.x/MovementRange;
            SY = Stick.rectTransform.localPosition.y/MovementRange;

            float SXX = Vector2.ClampMagnitude(new Vector2(SX, SY), 1).x;
            float SYY = Vector2.ClampMagnitude(new Vector2(SX, SY), 1).y;
            float angle = Mathf.Rad2Deg * Mathf.Atan2(SY, SX);
            if (angle < 0)
                angle += 360;




            resetOptions();

            if (new Vector2(SX, SY).magnitude < 0.5) {
                attack = true;
                //ActionCenter.color = new Color(1, 1, 1, 1);
                ActionCenterPushed.enabled = true;
            }
            else if ((angle >= 0 && angle < 54) || (angle >= 270 && angle < 360)) {
                overreact = true;
                ActionRight.color = new Color(1, 1, 1, 1);
            }
            else if ((angle >= 126 && angle < 270)) {
                taunt = true;
                ActionLeft.color = new Color(1, 1, 1, 1);
            }
            else if((angle >= 54 && angle < 126)) {
                cancel = true;
                ActionBottom.color = new Color(1, 1, 1, 1);
            }
            //else if (SX < 0 && -SX > Mathf.Abs(SY)) {
            //    taunt = true;
            //    ActionLeft.color = new Color(1, 1, 1, 1);
            //}
            //else if (SY > 0 && SY > Mathf.Abs(SX)) {
            //    //question = true;
            //    //ActionTop.color = new Color(1, 1, 1, 1);
            //    resetOptions();
            //}
            //else if (SY < 0 && -SY > Mathf.Abs(SX)) {
            //    cancel = true;
            //    ActionBottom.color = new Color(1, 1, 1, 1);
            //}
            //else if (SY < -0.2f) {
            //        ActionBottom.color = new Color(1, 1, 1, 1);
            //        ActionTop.color = new Color(0.5f, 0.5f, 0.5f, 1);
            //}
            //else {
            //    ActionTop.color = new Color(0.5f, 0.5f, 0.5f, 1);
            //    ActionBottom.color = new Color(0.5f, 0.5f, 0.5f, 1);
            //}

            //if (SY > 0) {
            //    Stick.color = new Color(1- SY/2, 1-SY, 1, 1);
            //}
            //else if(SY < 0) {
            //    Stick.color = new Color(1, 1+SY/2 , 1+SY/2, 1);
            //}

            //t.text = "x: " + Math.Round(SX, 2) + "y: " + Math.Round(SY, 2);

            moving = true;

            // Finally, we update our virtual axis
            HorizintalAxis.Value = horizontalValue;
            VerticalAxis.Value = verticalValue;
        }

        public void resetOptions() {
            attack = false;
            taunt = false;
            overreact = false;
            cancel = false;
            //question = false;
            //ActionTop.color = new Color(0.5f, 0.5f, 0.5f, 1);
            ActionBottom.color = new Color(0.5f, 0.5f, 0.5f, 1);
            ActionLeft.color = new Color(0.5f, 0.5f, 0.5f, 1);
            ActionRight.color = new Color(0.5f, 0.5f, 0.5f, 1);
            ActionCenterPushed.enabled = false;

        }

        public void rotateActionButton() {
            Rotation += 90;
            if(Rotation == 360) {
                Rotation = 0;
            }

            ActionUI.transform.eulerAngles = new Vector3(0,0,Rotation);
        }

        public void OnPointerUp(PointerEventData eventData)
        {

            moving = false;
            // When we lift our finger, we reset everything to the initial state
            _baseTransform.anchoredPosition = _initialBasePosition;
            _stickTransform.anchoredPosition = _initialStickPosition;
            _intermediateStickPosition = _initialStickPosition;

            HorizintalAxis.Value = VerticalAxis.Value = 0f;

            if (attack) {
                action_attack();
            }
            else if (taunt) {
                action_taunt();
            }
            else if (overreact) {
                action_overreact();
            }
            else if (question) {
                action_question();
            }
            else if (cancel) {
                action_cancel();
            }
            resetOptions();


            // We also hide it if we specified that behaviour
            if (HideOnRelease)
            {
                Hide(true);
            }
        }

        private void action_attack()
        {
            t.text = "Attack";
            t.color = new Color(1, 0, 0, 1);
            Vibration.Vibrate(50);
            playerActionController.HandleAttack();
            //playerScript.attack();
        }

        private void action_taunt()
        {
            t.text = "Taunt";
            t.color = new Color(0, 0, 1, 1);
            Vibration.Vibrate(50);
            //playerScript.taunt();
            playerActionController.HandleTaunt();
        }
        private void action_overreact() {
            t.text = "overreact";
            t.color = new Color(0, 1, 0, 1);
            playerActionController.HandleOverreact();

        }
        private void action_question() {
            t.text = "question";
            t.color = new Color(1, 1, 0, 1);
        }
        private void action_cancel() {
            t.text = "cancelled";
            t.color = new Color(0, 1, 1, 1);
        }

        public void OnPointerDown(PointerEventData eventData) {


            SX = SY = 0;
            ActionTop.color = new Color(0.5f, 0.5f, 0.5f, 1);
            ActionBottom.color = new Color(0.5f, 0.5f, 0.5f, 1);
            //Debug.Log("JFIDOAWJDIOWA");
            // When we press, we first want to snap the joystick to the user's finger
            if (SnapsToFinger)
            {
                CurrentEventCamera = eventData.pressEventCamera ?? CurrentEventCamera;

                Vector3 localStickPosition;
                Vector3 localBasePosition;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(_stickTransform, eventData.position,
                    CurrentEventCamera, out localStickPosition);
                RectTransformUtility.ScreenPointToWorldPointInRectangle(_baseTransform, eventData.position,
                    CurrentEventCamera, out localBasePosition);

                _baseTransform.position = localBasePosition;
                _stickTransform.position = localStickPosition;
                _intermediateStickPosition = _stickTransform.anchoredPosition;
            }
            //else
            //{
            //    OnDrag(eventData);
            //}
            OnDrag(eventData);

            // We also want to show it if we specified that behaviour
            if (HideOnRelease)
            {
                Hide(false);
            }
        }

        /// <summary>
        /// Simple "Hide" behaviour
        /// </summary>
        /// <param name="isHidden">Whether the joystick should be hidden</param>
        private void Hide(bool isHidden)
        {
            ActionUI.gameObject.SetActive(!isHidden);
            //ActionTop.gameObject.SetActive(!isHidden);
            //ActionBottom.gameObject.SetActive(!isHidden);
            //Stick.gameObject.SetActive(!isHidden);
            //Stick.gameObject.GetComponent<Image>().enabled = isHidden;
        }
    }
}
