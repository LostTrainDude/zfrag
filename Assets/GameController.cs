﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public const string DRAGGABLE_TAG = "UIDraggable";

    private bool _dragging = false;

    private Vector2 _originalPosition;
    private int _originalSiblingIndex;

    private Transform _objectToDrag;
    private Image _objectToDragImage;

    List<RaycastResult> _hitObjects = new List<RaycastResult>();

    GameObject GetObjectUnderMouse()
    {
        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        EventSystem.current.RaycastAll(pointer, _hitObjects);
        if (_hitObjects.Count <= 0) return null;

        return _hitObjects[0].gameObject;
    }

    Transform GetDraggableTransformUnderMouse()
    {
        GameObject clickedObject = GetObjectUnderMouse();

        if (clickedObject != null && clickedObject.tag == DRAGGABLE_TAG)
        {
            return clickedObject.transform;
        }

        return null;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Defragger.Instance.IsAutoDefragging) return;

        if (Input.GetMouseButtonDown(0))
        {
            _objectToDrag = GetDraggableTransformUnderMouse();
            if (_objectToDrag != null)
            {
                _dragging = true;
                _originalSiblingIndex = _objectToDrag.GetSiblingIndex();
                //_objectToDrag.SetAsLastSibling();
                _originalPosition = _objectToDrag.position;
                _objectToDragImage = _objectToDrag.GetComponent<Image>();
                _objectToDragImage.raycastTarget = false;
                Defragger.Instance.FooterText.text = Defragger.Instance.ChangeRandomFooterText();
            }
        }

        if (_dragging)
        {
            _objectToDrag.position = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_objectToDrag != null)
            {
                Transform objectToReplace = GetDraggableTransformUnderMouse();
                int objectToReplaceSiblingIndex;

                if (objectToReplace != null)
                {
                    objectToReplaceSiblingIndex = objectToReplace.GetSiblingIndex();

                    _objectToDrag.position = objectToReplace.position;
                    objectToReplace.position = _originalPosition;

                    _objectToDrag.SetSiblingIndex(objectToReplaceSiblingIndex);
                    objectToReplace.SetSiblingIndex(_originalSiblingIndex);

                    Defragger.Instance.FooterText.text = Defragger.Instance.ChangeRandomFooterText();

                    Defragger.Instance.CheckGrid();
                    Defragger.Instance.RefreshFillBar();
                    AudioController.instance.PlayClack();
                    AudioController.instance.PlaySeekSound();
                }
                else
                {
                    _objectToDrag.position = _originalPosition;
                }

                _objectToDragImage.raycastTarget = true;
                _objectToDrag = null;
            }

            _dragging = false;
        }
    }
}

