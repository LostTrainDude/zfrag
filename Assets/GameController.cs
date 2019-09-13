using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private TextMeshProUGUI _objectToDragText;

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

    // Update is called once per frame
    void Update()
    {
        if (Defragger.instance.State == DefraggerState.AUTODEFRAG) return;

        if (Input.GetMouseButtonDown(0))
        {
            _objectToDrag = GetDraggableTransformUnderMouse();
            if (_objectToDrag != null)
            {
                _dragging = true;
                _originalSiblingIndex = _objectToDrag.GetSiblingIndex();
                _originalPosition = _objectToDrag.position;
                _objectToDragText = _objectToDrag.GetComponent<TextMeshProUGUI>();
                _objectToDragText.raycastTarget = false;
                Defragger.instance.FooterText.text = Defragger.instance.ChangeRandomFooterText();
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

                    Defragger.instance.FooterText.text = Defragger.instance.ChangeRandomFooterText();

                    if (Defragger.instance.State != DefraggerState.FREEPAINTING)
                    {
                        Defragger.instance.ScanGrid();
                        Defragger.instance.UpdateProgressBar();
                    }

                    AudioController.instance.PlayClack();
                    AudioController.instance.PlaySeekSound();
                }
                else
                {
                    _objectToDrag.position = _originalPosition;
                }

                _objectToDragText.raycastTarget = true;
                _objectToDrag = null;
            }

            _dragging = false;
        }
    }
}

